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
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public CartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{token}")]
        public JsonResult Get(string token)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"select convert(bit, case when dbo.Cart.ExpireOn > '" + today + @"' then 0 else 1 " +
                    "end) as Expired, dbo.Books.BookId, dbo.Books.BookName, dbo.Books.Condition, dbo.Books.MRP, dbo.Books.PricePerWeek " +
                    "from dbo.Books right join dbo.Cart on dbo.Cart.BookId = dbo.Books.BookId where dbo.Cart.UserId in (select UserId from"+
                    " dbo.Users where Token ='" + token + "' and Expire >'" + today + "')" + " and dbo.Books.Deleted = 0";
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
        public JsonResult Post(Cart cart)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string expireOn = DateTime.Today.AddDays(4).ToString("yyyy-MM-dd");
            string query = @"insert into dbo.Cart (BookId,UserId,ExpireOn) values ("
                + cart.BookId + ",(select UserId from dbo.Users where Token ='"
                + cart.Token + "' and Expire >'" + today + "'),'" + expireOn + "')";
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        reader = command.ExecuteReader();
                        reader.Close();
                    }
                }
                catch(SqlException)
                {
                    query = @"update dbo.Cart set BookId =" + cart.BookId + ",UserId in (select UserId from dbo.Users where Token ='"
                        + cart.Token + "' and Expire >'" + today + "'),ExpireOn ='" + expireOn + @"'";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        reader = command.ExecuteReader();
                        reader.Close();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return new JsonResult("Book Added");
        }

        [HttpDelete]
        public JsonResult Delete(Cart cart)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string query = @"delete from dbo.Cart where BookId =" + cart.BookId + " and UserId in (select UserId from dbo.Users where Token ='"
                + cart.Token + "' and Expire >'" + today + "')";
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
            return new JsonResult("Book Removed");
        }
    }
}
