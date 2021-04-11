using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookRentalAPI.Models
{
    public class Rating
    {
        public int BookId { get; set; }
        public int Stars { get; set; }
        public string Token { get; set; }
    }
}
