namespace PadelSimple.Web.Infrastructure;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        _logger.LogInformation("HTTP {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
        await _next(ctx);
    }
}
