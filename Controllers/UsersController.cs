﻿using BookRentalAPI.Models;
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
        [HttpGet]
        public JsonResult Get(User user)
        {
            string query = @"select UserId, Email, FirstName, LastName, MobileNumber from dbo.Users where Email = '" 
                    + user.Email + "' and Password = '" + user.Password + "' and Deleted != 1";
            DataTable table = new DataTable();
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    reader = command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    connection.Close();
                } 
            }
            if(table.Rows.Count == 0)
            {
                return new JsonResult(new { error = "Email or password is incorrect." });
            }
            return new JsonResult(table);
        }

        [Route("{id}")]
        [HttpGet]
        public JsonResult Get(string id)
        {
            string query = @"select UserId, Email, FirstName, LastName, MobileNumber from dbo.Users where UserId = '" + id + "' and Deleted = 0";
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
            string query = @"insert into dbo.Users (Deleted, Email, FirstName, LastName, MobileNumber, Password) 
                        values ('False','" + user.Email + "','" + user.FirstName + "','"
                        + user.LastName + "','" + user.MobileNumber + "','" + user.Password + "')";
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
            string query = @"update dbo.Users set
                    Email = '" + user.Email + @"', 
                    FirstName = '" + user.FirstName + @"', 
                    LastName = '" + user.LastName + @"', 
                    MobileNumber = '" + user.MobileNumber + @"'
                    where UserId = " + user.UserId + @" and Deleted = 0";
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
            if (!CheckPassword(request.UserId, request.OldPassword))
            {
                return new JsonResult(new { error = "Password is not correct." });
            }
            string query = @"update dbo.Users set
                    Password = '" + request.NewPassword + @"'
                    where UserId = " + request.UserId + @"";
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
            if (!CheckPassword(user.UserId, user.Password))
            {
                return new JsonResult(new { error = "Password is not correct." });
            }
            string query1 = @"update dbo.Users set Deleted = 1
                    where UserId = " + user.UserId + @"";
            string query2 = @"update dbo.Addresses set Deleted = 1
                    where UserId = " + user.UserId + @"";
            string query3 = @"update dbo.Books set Deleted = 1
                    where OwnerId = " + user.UserId + @"";
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

        private bool CheckPassword(int id,string password)
        {
            string query = @"select * from dbo.Users where
                    Password = '" + password + @"'
                    and UserId = " + id + @"";
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
