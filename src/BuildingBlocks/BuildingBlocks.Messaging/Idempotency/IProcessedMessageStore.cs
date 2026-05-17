namespace BuildingBlocks.Messaging.Idempotency;

public interface IProcessedMessageStore
{
    Task<bool> TryStartProcessingAsync(
        Guid eventId,
        string consumerName,
        CancellationToken cancellationToken);
}
