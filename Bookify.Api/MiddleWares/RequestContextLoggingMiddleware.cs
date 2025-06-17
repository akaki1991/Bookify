using Serilog.Context;

namespace Bookify.Api.MiddleWares;

public class RequestContextLoggingMiddleWare(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", GetCorreleationId(context)))
        { 
            return _next(context);
        }
    }

    private static string GetCorreleationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}
