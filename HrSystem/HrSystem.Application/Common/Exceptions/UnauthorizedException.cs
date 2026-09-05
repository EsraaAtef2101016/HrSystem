using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HrSystem.Application.Common.Exceptions;
namespace HrSystem.Application.Common.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message, string errorCode = "UNAUTHORIZED")
        : base(401, errorCode, message)
    {
    }

    public UnauthorizedException(int status, string Code, string Message) : base(status, Code, Message)
    {
    }
}
