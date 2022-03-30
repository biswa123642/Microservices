using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoginMicroservice.Models
{
    public class RequestResponse
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public bool Success { get; set; }
        public int StatusCode { get; set; }
    }
}
