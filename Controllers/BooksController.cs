using BookRentalAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        [HttpGet("{token}/{offset}")]
        public JsonResult Get(List<int> categories,string token, int offset )
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"select BookId, BookName, Condition, CoverImageName,convert(bit, case when BookId in (select BookId " +
                "from dbo.Cart where UserId in (Select UserId from dbo.Users where Token  ='" + token + @"' and Expire > '" + today +
                "') and ExpireOn > '" + today + @"') then 1 else 0 end) as InCart,convert(bit, case when BookId in (select BookId from" +
                " dbo.WishList where UserId in(Select UserId from dbo.Users where Token  ='" + token + @"' and Expire > '" + today +
                "')) then 1 else 0 end) as InWishList, MRP, PricePerWeek from dbo.Books where OwnerId not in (Select UserId from dbo.Users" +
                " where Token  ='" + token + @"' and Expire > '" + today + "') and Deleted = 0 and BookId not in " +
                "(select BookId from dbo.Rental where EndDate >= '" + today + @"' and BorrowerId not in (Select UserId from dbo.Users where Token  ='"
                + token + @"' and Expire > '" + today + "')" + ((categories.Count == 0) ? @")" : @") and BookId in (select distinct BookId from" +
                " dbo.BookCategories where CategoryId in ({0})) ") + @"order by BookId desc offset " + offset + @"rows fetch next 20 rows only";
            if(categories.Count != 0)
            {
                string ids = categories[0].ToString();
                for (int i = 1; i < categories.Count; i++)
                {
                    ids += "," + categories[i].ToString();
                }
                query = string.Format(query, ids);
            }
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

        [HttpGet("MyBooks/{token}")]
        public JsonResult Get(string token)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"select BookId, BookName, Condition, CoverImageName, MRP, PricePerWeek from dbo.Books where OwnerId" +
                " in (Select UserId from dbo.Users where Token  ='" + token + @"' and Expire > '" + today + "') and Deleted = 0";
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
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"insert into dbo.Books (BookName, Condition, Deleted, CoverImageName, MRP, OwnerId, PricePerWeek)"
                    + " output INSERTED.BookId values ('" + book.BookName + "'," + book.Condition + ", 0,'" + book.CoverImageName + "',"
                    + book.MRP + ",(Select UserId from dbo.Users where Token ='" + book.Token + "' and Expire >'" + today + "'),"
                    + book.PricePerWeek + ")";
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            DataTable table = new DataTable();
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
            return new JsonResult(new { BookId = (table.Rows[0])["BookId"] });
        }

        [HttpPut]
        public JsonResult Put(Book book)
        {
            string query = @"update dbo.Books set BookName ='" + book.BookName + "', Condition =" + book.Condition + ", CoverImageName ='"
                    + book.CoverImageName + "', MRP =" + book.MRP + ", PricePerWeek = "
                    + book.PricePerWeek + " where BookId =" + book.BookId + @"";
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

        [Route("uploadimage")]
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