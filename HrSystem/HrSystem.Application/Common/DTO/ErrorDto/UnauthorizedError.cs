using FluentResults;
using Microsoft.AspNetCore.Http;

namespace HrSystem.Application.Common.DTO.ErrorDto;

public class UnauthorizedError : Error
{
    public UnauthorizedError(string message = "You are not authorized to perform this action.") : base(message)
    {
        WithMetadata("Code", "UNAUTHORIZED");
        WithMetadata("StatusCode", StatusCodes.Status401Unauthorized);
    }
}
