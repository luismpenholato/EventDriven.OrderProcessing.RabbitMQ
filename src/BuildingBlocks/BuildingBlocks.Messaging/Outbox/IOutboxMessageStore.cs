namespace BuildingBlocks.Messaging.Outbox;

public interface IOutboxMessageStore
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);

    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken);

    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken);

    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken);
}
