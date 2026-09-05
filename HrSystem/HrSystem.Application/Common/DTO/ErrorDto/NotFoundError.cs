using FluentResults;
using Microsoft.AspNetCore.Http;

namespace HrSystem.Application.Common.DTO.ErrorDto;

public class NotFoundError : Error
{
    public NotFoundError(string message = "The requested resource was not found.") : base(message)
    {
        WithMetadata("Code", "Not_Found_Error");
        WithMetadata("StatusCode", StatusCodes.Status404NotFound);
    }
}
