using BuildingBlocks.Contracts;
using Serilog.Context;

namespace BuildingBlocks.Observability.Correlation;

public sealed class ConsumerCorrelationScope : IDisposable
{
    private readonly IDisposable _logScope;

    public ConsumerCorrelationScope(ICorrelationContext correlationContext, IntegrationEvent integrationEvent)
    {
        correlationContext.SetFromIntegrationEvent(integrationEvent);
        _logScope = LogContext.PushProperty("CorrelationId", correlationContext.CorrelationId!);
    }

    public void Dispose() => _logScope.Dispose();
}
