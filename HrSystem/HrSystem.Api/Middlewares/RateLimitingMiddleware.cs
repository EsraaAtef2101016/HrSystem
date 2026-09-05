using HrSystem.Application.Common.Exceptions;
namespace HrSystem.Api.Middlewares;
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static int _Counter = 0;
    private static DateTime _LastRequestTime = DateTime.Now;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task Invoke(HttpContext context)
    {
        if ((DateTime.Now.Subtract(_LastRequestTime)).Seconds > 5)
        {
            _Counter = 1;
            _LastRequestTime = DateTime.Now;
            await _next(context);
        }
        else
        {
            _Counter++;
            if (_Counter > 500)
            {
                _LastRequestTime = DateTime.Now;
                throw new RateLimitException("Too many requests. Please try again later.");

            }
            else
            {
                _LastRequestTime = DateTime.Now;
                await _next(context);
            }
        }


    }
}
