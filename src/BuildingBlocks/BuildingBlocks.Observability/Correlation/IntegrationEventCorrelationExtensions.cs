using BuildingBlocks.Contracts;

namespace BuildingBlocks.Observability.Correlation;

public static class IntegrationEventCorrelationExtensions
{
    public static void SetFromIntegrationEvent(this ICorrelationContext correlationContext, IntegrationEvent integrationEvent)
    {
        correlationContext.Set(CorrelationIdNormalizer.FromGuid(integrationEvent.CorrelationId));
    }
}
