using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AppFactory.Framework.Api.AspNetCore.Middleware;

/// <summary>
/// Middleware for request/response logging
/// Logs request details and performance metrics
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = context.TraceIdentifier }))
        {
            _logger.LogInformation("{Method} {Path}{QueryString}", 
                context.Request.Method, 
                context.Request.Path, 
                context.Request.QueryString);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("Request completed in {ElapsedMs}ms with status {StatusCode}", 
                    stopwatch.ElapsedMilliseconds, 
                    context.Response.StatusCode);
            }
        }
    }
}
