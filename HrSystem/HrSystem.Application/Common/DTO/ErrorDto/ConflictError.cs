using FluentResults;
using Microsoft.AspNetCore.Http;


namespace HrSystem.Application.Common.DTO.ErrorDto;

public class ConflictError : Error
{
    public ConflictError(string message = "A concurrent claim attempt occurred. Please try again.") : base(message)
    {
        WithMetadata("Code", "Conflict");
        WithMetadata("StatusCode", StatusCodes.Status409Conflict);
    }
}
