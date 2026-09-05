using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrSystem.Application.Common.Exceptions;
namespace HrSystem.Application.Common.Exceptions;


public class RateLimitException : AppException
{
    public RateLimitException(string message, string errorCode = "RateLimitExceeded")
        : base(429, errorCode, message)
    {
    }

}

