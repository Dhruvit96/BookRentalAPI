using BookRentalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace BookRentalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("Login")]
        [HttpPost]
        public JsonResult Login(User user)
        {
            string query = @"select UserId from dbo.Users where Email = '" + user.Email + "' and Password = '"
                + user.Password + "' and Deleted != 1";
            string expire = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd");
            string checkToken = @"select UserId from dbo.Users where Token = '{0}'";
            string tokenGenration = @"Update dbo.Users set Token ='{0}', Expire ='" + expire + @"' where UserId = {1}";
            string getUser = @"Select Email,FirstName,LastName,MobileNumber,Token from dbo.Users where UserId = {0}";
            DataTable table = new DataTable();
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            string token;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    reader = command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                }
                if (table.Rows.Count == 0)
                {
                    connection.Close();
                    return new JsonResult(new { error = "Email or password is incorrect." });
                }
                else
                {
                    string userId = (table.Rows[0])["UserId"].ToString();
                    token = Guid.NewGuid().ToString();
                    using (SqlCommand command = new SqlCommand(string.Format(checkToken,token), connection))
                    {
                        reader = command.ExecuteReader();
                        table = new DataTable();
                        table.Load(reader);
                        reader.Close();
                    }
                    while(table.Rows.Count != 0)
                    {
                        token = Guid.NewGuid().ToString();
                        using (SqlCommand command = new SqlCommand(string.Format(checkToken, token), connection))
                        {
                            reader = command.ExecuteReader();
                            table = new DataTable();
                            table.Load(reader);
                            reader.Close();
                        }
                    }
                    using (SqlCommand command = new SqlCommand(string.Format(tokenGenration,token,userId), connection))
                    {
                        reader = command.ExecuteReader();
                        reader.Close();
                    }
                    using(SqlCommand command = new SqlCommand(string.Format(getUser, userId), connection))
                    {
                        reader = command.ExecuteReader();
                        table = new DataTable();
                        table.Load(reader);
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return new JsonResult(table);
        }

        [Route("{token}")]
        [HttpGet]
        public JsonResult Get(string token)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"select Email, FirstName, LastName, MobileNumber,Token from dbo.Users where Token = '" + token
                + "'and Expire > '" + today + @"' and Deleted = 0";
            DataTable table = new DataTable();
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    reader = command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    connection.Close();
                }
            }
            if (table.Rows.Count == 0)
            {
                return new JsonResult(new { error = "User does not exist." });
            }
            return new JsonResult(table);
        }

        [Route("Register")]
        [HttpPost]
        public JsonResult Post(User user)
        {
            string query = @"insert into dbo.Users (Deleted, Email, FirstName, LastName, MobileNumber, Password)" +
                        "values ('False','" + user.Email + "','" + user.FirstName + "','" + user.LastName
                        + "','" + user.MobileNumber + "','" + user.Password + "')";
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        reader = command.ExecuteReader();
                        reader.Close();
                        connection.Close();
                    }
                    catch(SqlException exception)
                    {
                        if(exception.Number == 2601)
                            return new JsonResult(new { error = "Email or Mobile number already exist." });
                        else
                            return new JsonResult(new { error = "Something went wrong try again later." });
                    }
                }
            }
            return new JsonResult("Registration succeed");
        }

        [HttpPut]
        public JsonResult Put(User user)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"update dbo.Users set
                    Email = '" + user.Email + @"', 
                    FirstName = '" + user.FirstName + @"', 
                    LastName = '" + user.LastName + @"', 
                    MobileNumber = '" + user.MobileNumber + @"'
                    where Token = '" + user.Token + @"' and Deleted = 0 and Expire > '" + today + "'";
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        reader = command.ExecuteReader();
                        reader.Close();
                        connection.Close();
                    }
                    catch (SqlException exception)
                    {
                        if (exception.Number == 2601)
                            return new JsonResult(new { error = "Email or Mobile number already exist." });
                        else
                            return new JsonResult(new { error = "Something went wrong try again later." });
                    }
                }
            }
            return new JsonResult("Profile Updated");
        }

        [Route("UpdatePassword")]
        [HttpPut]
        public JsonResult Put(UpdatePasswordRequest request)
        {
            if (!CheckPassword(request.Token, request.OldPassword))
            {
                return new JsonResult(new { error = "Password is not correct." });
            }
            string query = @"update dbo.Users set Password = '" + request.NewPassword + @"' where Token ='" + request.Token + @"'";
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    reader = command.ExecuteReader();
                    reader.Close();
                    connection.Close();
                }
            }
            return new JsonResult("Password Updated");
        }

        [HttpDelete]
        public JsonResult Delete(User user)
        {
            if (!CheckPassword(user.Token, user.Password))
            {
                return new JsonResult(new { error = "Password is not correct." });
            }
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query1 = @"update dbo.Users set Deleted = 1
                    where Token = '" + user.Token + @"' and Expire >'" + today + @"";
            string query2 = @"update dbo.Addresses set Deleted = 1
                    where UserId in (select UserId from dbo.Users where Token = '" + user.Token + "' ) + @";
            string query3 = @"update dbo.Books set Deleted = 1
                    where OwnerId in (select UserId from dbo.Users where Token = '" + user.Token + "' ) + @";
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query1, connection))
                {
                    reader = command.ExecuteReader();
                    reader.Close();
                }
                using (SqlCommand command = new SqlCommand(query2, connection))
                {
                    reader = command.ExecuteReader();
                    reader.Close();
                }
                using (SqlCommand command = new SqlCommand(query3, connection))
                {
                    reader = command.ExecuteReader();
                    reader.Close();
                    connection.Close();
                }
            }
            return new JsonResult("User Deleted");
        }

        private bool CheckPassword(string token,string password)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"select * from dbo.Users where" +
                    " Password = '" + password + @"'
                    and Token = '" + token + @"'
                    and Expire > '" + today + @"'";
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            DataTable table = new DataTable();
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    reader = command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    connection.Close();
                }
            }
            if (table.Rows.Count == 0)
            {
                return false;
            }
            return true;
        }
    }
}
