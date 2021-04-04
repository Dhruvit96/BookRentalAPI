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
    public class RatingController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public RatingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{bookId}")]
        public JsonResult Get(string bookId)
        {
            string query = @"select coalesce(convert(decimal(10,2),avg(cast(Stars as decimal))),0) as Stars from dbo.Ratings where BookId =" + bookId + @" and deleted = 0";
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
            return new JsonResult(new { Stars = (table.Rows[0])["Stars"] });
        }

        [HttpGet("{bookId}/{userId}")]
        public JsonResult Get(string bookId,string userId)
        {
            string query = @"select coalesce(Stars,0) as Stars from dbo.Ratings where BookId =" + bookId + " and UserId =" + userId + @" and deleted = 0";
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
            return new JsonResult(new { Stars = (table.Rows.Count == 0 ? 0 : (table.Rows[0])["Stars"]) });
        }

        [HttpPost]
        public JsonResult Post(Rating rating)
        {
            string query = @"insert into dbo.Ratings (BookId, UserId, Deleted, Stars) values ("
                        + rating.BookId + "," + rating.UserId + ", 0," + rating.Stars + ")";
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
            return new JsonResult("Rating added");
        }

        [HttpPut]
        public JsonResult Put(Rating rating)
        {
            string query = @"update dbo.Ratings set Stars = " + rating.Stars + " where BookId =" + rating.BookId + " and UserId =" + rating.UserId + @"";
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
            return new JsonResult("Rating Updated");
        }

        [HttpDelete]
        public JsonResult Delete(Rating rating)
        {
            string query = @"set dbo.Ratings Deleted = 1 where BookId =" + rating.BookId + " and UserId =" + rating.UserId + @"";
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
            return new JsonResult("Rating Removed");
        }
    }
}
