using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoginMicroservice.Models
{
    public class Usertoken
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Token { get; set; }
    }
}
