using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrSystem.Application.Extensions
{
    public static class ValidationExtensions
    {
        public static async Task<Result<TResponse>> ValidateRequestAsync<TRequest, TResponse>(
          this IValidator<TRequest> validator,
          TRequest request,
          int statusCode = StatusCodes.Status400BadRequest)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e =>
                  new Error(e.ErrorMessage)
                    .WithMetadata("Code", e.ErrorCode)
                    .WithMetadata("PropertyName", e.PropertyName)
                    .WithMetadata("StatusCode", statusCode));

                return Result.Fail<TResponse>(errors);
            }

            return Result.Ok<TResponse>(default!);
        }



        public static Result<TResponse> ValidateEnum<TEnum, TResponse>(
            string? value,
            string propertyName,
            int statusCode = StatusCodes.Status400BadRequest)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value) ||
                !Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsedEnum) ||
                !Enum.IsDefined(typeof(TEnum), parsedEnum))
            {
                var error = new Error($"The field '{propertyName}' contains an invalid value.")
                    .WithMetadata("Code", "INVALID_ENUM_VALUE")
                    .WithMetadata("PropertyName", propertyName)
                    .WithMetadata("StatusCode", statusCode);

                return Result.Fail<TResponse>(error);
            }

            return Result.Ok<TResponse>(default!);
        }

    }
}