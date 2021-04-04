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
    public class CategoriesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public CategoriesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = @"select * from dbo.Categories";
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

        [HttpPost("{bookId}")]
        public JsonResult Post(List<int> categoryIds,string bookId)
        {
            string query = @"insert into dbo.BookCategories (BookId, CategoryId) values {0}";
            string ids = "(" + bookId + "," + categoryIds[0].ToString() + ")";
            for (int i = 1; i < categoryIds.Count; i++)
            {
                ids += "," + "(" + bookId + "," + categoryIds[i].ToString() + ")";
            }
            query = string.Format(query, ids);
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
            return new JsonResult("Categories Added");
        }

        [HttpDelete("{bookId}")]
        public JsonResult Delete(List<int> categoryIds, string bookId)
        {
            string connectionString = _configuration.GetConnectionString("BookRentalCon");
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                for(int i = 0; i < categoryIds.Count; i++)
                {
                    string query = @"delete from dbo.BookCategories where BookId =" + bookId + " and CategoryId =" + categoryIds[i].ToString() + @"";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        reader = command.ExecuteReader();
                        reader.Close();
                    }
                }
            }
            return new JsonResult("Categories Removed");
        }

    }
}
