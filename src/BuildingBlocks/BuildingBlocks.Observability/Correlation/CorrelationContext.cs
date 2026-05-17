namespace BuildingBlocks.Observability.Correlation;

public sealed class CorrelationContext : ICorrelationContext
{
    private string? _correlationId;

    public string? CorrelationId => _correlationId;

    public bool HasCorrelationId => !string.IsNullOrWhiteSpace(_correlationId);

    public void Set(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            throw new ArgumentException("Correlation id is required.", nameof(correlationId));

        _correlationId = correlationId.Trim();
    }

    public Guid GetRequiredGuid()
    {
        if (!HasCorrelationId)
            throw new InvalidOperationException("Correlation id has not been set for the current scope.");

        return CorrelationIdNormalizer.ToGuid(_correlationId!);
    }
}
