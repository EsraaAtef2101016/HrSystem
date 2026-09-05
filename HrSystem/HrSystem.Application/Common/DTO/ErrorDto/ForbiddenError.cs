using FluentResults;
using Microsoft.AspNetCore.Http;

namespace HrSystem.Application.Common.DTO.ErrorDto;

public class ForbiddenError : Error
{
    public ForbiddenError(string message = "Access denied. You do not have the required role.") : base(message)
    {
        WithMetadata("Code", "Forbidden_Error");
        WithMetadata("StatusCode", StatusCodes.Status403Forbidden);
    }
}
