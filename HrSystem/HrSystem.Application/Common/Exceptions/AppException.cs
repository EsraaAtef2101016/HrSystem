using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrSystem.Application.Common.Exceptions;

public class AppException : Exception
{
    public int Status { get; }
    public string code { get; }
    public string message { get; }
    public string correlationId = Guid.NewGuid().ToString();
    //code, message, correlationId
    public AppException(int status, string Code, string Message)
        : base(Message)
    {
        Status = status;
        code = Code;
        message = Message;

    }
}
