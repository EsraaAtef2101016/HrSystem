using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrSystem.Application.Common.Exceptions;
namespace HrSystem.Application.Common.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message, string errorCode = "FORBIDDEN")
        : base(403, errorCode, message)
    {
    }

}
