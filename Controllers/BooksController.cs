﻿using BookRentalAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace BookRentalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public BooksController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet("{userId}/{offset}")]
        public JsonResult Get(string userId, int offset)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"select BookId, BookName, Condition, CoverImageName,case when BookId in (select BookId from dbo.Rental where BorrowerId = "
                    + userId + @" and EndDate > '" + today + @"') then 1 else 0 end as InCart,
                    case when BookId in (select BookId from dbo.WishList where UserId = "
                    + userId + @") then 1 else 0 end as InWishList, MRP, PricePerWeek from dbo.Books where OwnerId != "
                    + userId + @" and Deleted = 0 order by BookId desc offset " + offset + @"rows fetch next 20 rows only";
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
        public JsonResult Post(Book book)
        {
            string query = @"insert into dbo.Books (BookName, Condition, Deleted, CoverImageName, MRP, OwnerId, PricePerWeek)
                    values ('" + book.BookName + "'," + book.Condition + ", 0,'" + book.CoverImageName + "',"
                    + book.MRP + "," + book.OwnerId + "," + book.PricePerWeek + ")";
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
            return new JsonResult("Book added");
        }

        [HttpPut]
        public JsonResult Put(Book book)
        {
            string query = @"update dbo.Books set BookName ='" + book.BookName + "', Condition =" + book.Condition + ", CoverImageName ='"
                    + book.CoverImageName + "', MRP =" + book.MRP + ", PricePerWeek = "
                    + book.PricePerWeek + " where BookId =" + book.BookId + @"";
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
            return new JsonResult("Book Updated");
        }

        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            string query = @"update dbo.Books set Deleted = 1 where BookId =" + id + @"";
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
            return new JsonResult("Book Deleted");
        }

        [Route("UploadImage")]
        [HttpPost]
        public JsonResult Post()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string extension = Path.GetExtension(postedFile.FileName);
                string random = Guid.NewGuid() + extension;
                var physicalPath = _env.ContentRootPath + "/Photos/" + random;
                while (System.IO.File.Exists(physicalPath))
                {
                    random = Guid.NewGuid() + extension;
                    physicalPath = _env.ContentRootPath + "/Photos/" + random;
                }
                using(var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }
                return new JsonResult(random);
            }
            catch (Exception)
            {
                return new JsonResult(new { error = "Can not add image."});
            }
        }
    }
}