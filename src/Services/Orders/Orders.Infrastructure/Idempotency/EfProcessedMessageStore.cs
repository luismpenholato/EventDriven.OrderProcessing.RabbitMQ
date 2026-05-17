using BuildingBlocks.Messaging.Idempotency;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Idempotency;

public sealed class EfProcessedMessageStore(OrdersDbContext dbContext) : IProcessedMessageStore
{
    public Task<bool> TryStartProcessingAsync(
        Guid eventId,
        string consumerName,
        CancellationToken cancellationToken) =>
        dbContext.TryStartProcessingAsync(eventId, consumerName, cancellationToken);
}
