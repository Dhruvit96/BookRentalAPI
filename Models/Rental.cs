using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookRentalAPI.Models
{
    public class Rental
    {
        public int RentalId { get; set; }
        public int BookId { get; set; }
        public int BorrowerID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool Paid { get; set; }
        public bool Returned { get; set; }
    }
}
