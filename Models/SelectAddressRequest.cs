﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookRentalAPI.Models
{
    public class SelectAddressRequest
    {
        public int UserId { get; set; }
        public int AddressId { get; set; }
    }
}
