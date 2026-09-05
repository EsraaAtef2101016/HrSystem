using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrSystem.Application.Common.Exceptions;
namespace HrSystem.Application.Common.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message, string errorCode = "NOT_FOUND")
        : base(404, errorCode, message)
    {
    }
}

