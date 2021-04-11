using BookRentalAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BookRentalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AddressesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{token}")]
        public JsonResult Get(string token)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"select Address1, Address2, City, PostalCode, Selected from dbo.Addresses where UserId in (select UserId from" +
                    " dbo.users where Token = '" + token + @"' and Expire>'" + today + "') and Deleted = 0 ";
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
            return new JsonResult(table);
        }

        [HttpPost]
        public JsonResult Post(Address address)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"insert into dbo.Addresses (Address1, Address2, City, Deleted, PostalCode, Selected, UserId)" +
                    " output INSERTED.AddressId values ('" + address.Address1 + "','" + address.Address2 + "','"
                    + address.City + "', 0,'" + address.PostalCode + "', 0,(select UserId from dbo.Users where Token ='" +
                    address.Token + "' and Expire >'" + today + "'))";
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        reader = command.ExecuteReader();
                        table.Load(reader);
                        reader.Close();
                    }
                }
                catch
                {

                    connection.Close();
                    return new JsonResult(new { Error = "Login session expired." });
                }
            }
            return new JsonResult(new { AddressId = (table.Rows[0])["AddressId"] });
        }

        [Route("Select")]
        [HttpPut]
        public JsonResult Put(SelectAddressRequest request)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"update dbo.Addresses set Selected = case when AddressId ="
                    + request.AddressId + @" then 1 else 0 end where UserId in (select UserId from" +
                    " dbo.users where Token = '" + request.Token + @"' and Expire >'" + today + "')";
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
            return new JsonResult("Address Selected");
        }

        [HttpPut]
        public JsonResult Put(Address address)
        {
            string query = @"update dbo.Addresses set Address1 ='" + address.Address1 + "', Address2 ='" + 
                    address.Address2 + "', City ='" + address.City + "', PostalCode = '" + 
                    address.PostalCode + "' where AddressId = " + address.AddressId + @"";
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
            return new JsonResult("Address Updated");
        }

        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            string query = @"update dbo.Addresses set Deleted = 1 where AddressId =" + id + @"";
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
            return new JsonResult("Address Deleted");
        }
    }
}
