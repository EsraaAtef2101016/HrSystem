using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HrSystem.Api.Filter;

public class LogActivityFilter(ILogger<LogActivityFilter> logger) : IAsyncActionFilter
    {
        private readonly ILogger _logger = logger;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Log the incoming request details
            var request = context.HttpContext.Request;
            _logger.LogInformation("Incoming Request on controller {ContextController} with Argument {JsonSerializerSerialize}", context.Controller, JsonSerializer.Serialize(context.ActionArguments));
            // Proceed to the next action in the pipeline
            var resultContext = await next();
            // Log the outgoing response details
            var response = resultContext.HttpContext.Response;
            _logger.LogInformation($"Outgoing Response: {response}");
        }
    }