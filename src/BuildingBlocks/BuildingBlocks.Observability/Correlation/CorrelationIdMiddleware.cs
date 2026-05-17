using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace BuildingBlocks.Observability.Correlation;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, ICorrelationContext correlationContext)
    {
        var correlationId = httpContext.Request.Headers[CorrelationIdNormalizer.HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = CorrelationIdNormalizer.Generate();
        else
            correlationId = correlationId.Trim();

        correlationContext.Set(correlationId);
        httpContext.Response.Headers[CorrelationIdNormalizer.HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(httpContext);
        }
    }
}
