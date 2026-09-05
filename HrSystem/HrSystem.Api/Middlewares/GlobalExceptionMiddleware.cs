using System.Net;
using System.Text.Json;
using HrSystem.Application.Common.Exceptions;

namespace HrSystem.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";


        int statusCode = (int)HttpStatusCode.InternalServerError;
        string messageError = "Internal Server Error from the custom middleware.";
        string Errorcode = "InternalServerError";




        if (exception is AppException appEx)
        {

            statusCode = appEx.Status;
            messageError = appEx.Message;
            Errorcode = appEx.code;
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            code = Errorcode,

            message = messageError
            ,
            correlationId = Guid.NewGuid().ToString()
        };
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
