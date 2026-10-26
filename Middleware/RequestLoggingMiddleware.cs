using System.Diagnostics;

namespace UserManagementAPI.Middleware;

// Logs HTTP method, request path, response status code (and time) for auditing.
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
        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        _logger.LogInformation("Incoming request: {Method} {Path}", method, path);

        await _next(context);

        sw.Stop();
        _logger.LogInformation("Outgoing response: {Method} {Path} -> {StatusCode} ({Elapsed} ms)",
            method, path, context.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}
