using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoginMicroservice.Models
{
    public class ServiceResponse
    {
        public bool IsSuccess { get; set; }
        public int UserId { get; set; }
    }
}
