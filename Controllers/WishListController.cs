﻿using BookRentalAPI.Models;
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
    public class WishListController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public WishListController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{userId}")]
        public JsonResult Get(string userId)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"select convert(bit, case when BookId in (select BookId from dbo.Rental where EndDate >= '" + today + @"') then 0 else 1 
                    end) as Available, BookId, BookName, Condition, CoverImageName,convert(bit, case when BookId in 
                    (select BookId from dbo.Rental where BorrowerId = " + userId +
                    @" and EndDate > '" + today + @"') then 1 else 0 end) as InCart,
                    convert(bit, 1) as InWishList, MRP, PricePerWeek from dbo.Books where BookId in (select BookId from dbo.WishList where UserId ="
                    + userId + @") and Deleted = 0";
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
        public JsonResult Post(WishList wish)
        {
            string query = @"insert into dbo.WishList (BookId,UserId) values ("
                        + wish.BookId + "," + wish.UserId + ")";
            Console.WriteLine(query);
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
            return new JsonResult("WishList Added");
        }

        [HttpDelete]
        public JsonResult Delete(WishList wish)
        {
            string query = @"delete from dbo.WishList where BookId =" + wish.BookId + " and UserId =" + wish.UserId + @"";
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    reader = command.ExecuteReader();
                    reader.Close();
                }
            }
            return new JsonResult("WishList Removed");
        }

    }
}