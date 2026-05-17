namespace BuildingBlocks.Contracts;

public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    public int EventVersion { get; init; } = IntegrationEventVersions.V1;
}
