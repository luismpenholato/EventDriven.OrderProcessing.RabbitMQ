namespace BuildingBlocks.Observability.Correlation;

public interface ICorrelationContext
{
    string? CorrelationId { get; }

    bool HasCorrelationId { get; }

    void Set(string correlationId);

    Guid GetRequiredGuid();
}
