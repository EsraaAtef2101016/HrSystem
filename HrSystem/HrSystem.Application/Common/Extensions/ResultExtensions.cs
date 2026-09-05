using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Application.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this ResultBase result)
        {
            if (result.IsSuccess)
                return new OkResult();

            var firstError = result.Errors.FirstOrDefault();
            string code = "GENERAL_ERROR";
            string message = firstError?.Message ?? "An error occurred.";
            string correlationId = Guid.NewGuid().ToString();
            int statusCode = StatusCodes.Status400BadRequest;

            if (firstError != null)
            {
                if (firstError.Metadata.TryGetValue("Code", out var codeVal) && codeVal != null)
                    code = codeVal.ToString()!;

                if (firstError.Metadata.TryGetValue("CorrelationId", out var correlationValue) && correlationValue != null)
                    correlationId = correlationValue.ToString()!;

                if (firstError.Metadata.TryGetValue("StatusCode", out var statusVal) && statusVal is int statusInt)
                    statusCode = statusInt;
            }

            var errorResponse = new { code, message, correlationId };

            return statusCode switch
            {
                StatusCodes.Status400BadRequest => new BadRequestObjectResult(errorResponse) { StatusCode = StatusCodes.Status400BadRequest},
                StatusCodes.Status404NotFound => new NotFoundObjectResult(errorResponse){ StatusCode = StatusCodes.Status404NotFound},
                StatusCodes.Status401Unauthorized => new ObjectResult(errorResponse) { StatusCode = StatusCodes.Status401Unauthorized },
                StatusCodes.Status403Forbidden => new ObjectResult(errorResponse) { StatusCode = StatusCodes.Status403Forbidden },
                StatusCodes.Status409Conflict => new ObjectResult(errorResponse) { StatusCode = StatusCodes.Status409Conflict },
                _ => new BadRequestObjectResult(errorResponse)
            };
        }
    }
}