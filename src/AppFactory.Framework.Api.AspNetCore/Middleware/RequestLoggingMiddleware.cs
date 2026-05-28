using Microsoft.AspNetCore.Http;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.Api.AspNetCore.Middleware;

/// <summary>
/// Middleware for request/response logging
/// Logs request details and performance metrics
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger?.AddTraceId(context.TraceIdentifier);
        _logger?.LogInfo($"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger?.LogInfo($"Request completed in {stopwatch.ElapsedMilliseconds}ms with status {context.Response.StatusCode}");
        }
    }
}
