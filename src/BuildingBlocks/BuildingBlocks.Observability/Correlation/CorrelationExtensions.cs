using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Observability.Correlation;

public static class CorrelationExtensions
{
    public static IServiceCollection AddCorrelationContext(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        return services;
    }

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
