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
    public class RentalController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public RentalController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{userId}")]
        public JsonResult Get(string userId)
        {
            string query = @"select dbo.Rental.RentalId, dbo.Books.BookId, dbo.Books.BookName, dbo.Books.Condition, dbo.Books.MRP, dbo.Books.PricePerWeek,
                    dbo.Rental.StartDate, dbo.Rental.EndDate, dbo.Rental.Returned from dbo.Books right join dbo.Rental on 
                    dbo.Rental.BookId = dbo.Books.BookId where dbo.Rental.BorrowerId = " + userId + " and dbo.Books.Deleted = 0";
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
        public JsonResult Post(RentalRequest request)
        {
            string startDate = DateTime.Today.ToString("yyyy-MM-dd");
            string endDate = DateTime.Today.AddDays(2 + request.Days).ToString("yyyy-MM-dd");
            string query = @"insert into dbo.Rental (BookId,BorrowerId,StartDate,EndDate,Returned) 
                        output INSERTED.RentalId values (" + request.BookId + "," + request.UserId + ",'" + startDate + "','" 
                        + endDate + "',0)";
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
            return new JsonResult(new { RentalId = (table.Rows[0])["RentalId"] });
        }

        [HttpPut]
        public JsonResult Put(RentalRequest request)
        {
            string getLastDateQuery = @"select EndDate from dbo.Rental where RentalId=" + request.RentalId + @"";
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(getLastDateQuery, connection))
                {
                    reader = command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                }
                string endDate = DateTime.Parse((table.Rows[0])["EndDate"].ToString()).AddDays(request.Days).ToString("yyyy-MM-dd");
                string query = @"update dbo.Rental set EndDate = '" + endDate + "' where RentalId =" + request.RentalId + @"";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    reader = command.ExecuteReader();
                    reader.Close();
                    connection.Close();
                }
            }
            return new JsonResult("Date Extended.");
        }

        [HttpPost("return/{rentalId}")]
        public JsonResult Return(string rentalId)
        {
            string query = @"update dbo.Rental set Returned = 1 where RentalId =" + rentalId + @"";
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
            return new JsonResult("Book Returned");
        }
    }
}
