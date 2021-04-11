using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookRentalAPI.Models
{
    public class Cart
    {
        public int BookId { get; set; }
        public string Token { get; set; }
    }
}
