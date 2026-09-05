using FluentResults;
using Microsoft.AspNetCore.Http;

namespace HrSystem.Application.Common.DTO.ErrorDto;

public class BadRequestError : Error
{
    public BadRequestError(string message = "The request is invalid.") : base(message)
    {
        WithMetadata("Code", "Bad_Request_Error");
        WithMetadata("StatusCode", StatusCodes.Status400BadRequest);
    }
}
