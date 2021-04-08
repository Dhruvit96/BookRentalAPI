using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookRentalAPI.Models
{
    public class RentalRequest
    {
        public int RentalId { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public int Days { get; set; }
    }
}
