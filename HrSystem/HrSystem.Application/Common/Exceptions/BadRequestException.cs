

namespace HrSystem.Application.Common.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message, string errorCode = "BAD_REQUEST")
        : base(400, errorCode, message)
    {
    }


}
