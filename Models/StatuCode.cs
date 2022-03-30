using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoginMicroservice.Models
{
    public enum StatuCode
    {
        Success = 10000,
        Error = 10001,
        NotFound = 10002,
        AlreadyExists = 10003,
        InvalidCredentials = 10004
    }
}
