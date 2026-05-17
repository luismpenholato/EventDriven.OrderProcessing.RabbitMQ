namespace BuildingBlocks.Messaging.Idempotency;

public sealed class ProcessedMessage
{
    public Guid EventId { get; set; }

    public string ConsumerName { get; set; } = null!;

    public DateTime ProcessedAt { get; set; }
}
