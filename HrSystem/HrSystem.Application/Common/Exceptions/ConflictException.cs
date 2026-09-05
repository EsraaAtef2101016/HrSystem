using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrSystem.Application.Common.Exceptions;

namespace HrSystem.Application.Common.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message, string errorCode = "CONFLICT")
        : base(409, errorCode, message)
    {
    }
}
