using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookRentalAPI.Models
{
    public class Rating
    {
        public int RatingId { get; set; }
        public int BookId { get; set; }
        public int Stars { get; set; }
        public int UserId { get; set; }
    }
}
