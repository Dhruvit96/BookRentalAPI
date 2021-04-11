using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookRentalAPI.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public int Condition { get; set; }
        public string CoverImageName { get; set; }
        public int MRP { get; set; }
        public string Token { get; set; }
        public int PricePerWeek { get; set; }
    }
}
