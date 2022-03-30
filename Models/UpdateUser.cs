using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoginMicroservice.Models
{
    public class UpdateUser
    {
        public string EmailAddress { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string BusinessInformation { get; set; }
    }
}
