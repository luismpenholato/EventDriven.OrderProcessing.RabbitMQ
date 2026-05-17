using BuildingBlocks.Messaging.Idempotency;
using Inventory.Infrastructure.Persistence;

namespace Inventory.Infrastructure.Idempotency;

public sealed class EfProcessedMessageStore(InventoryDbContext dbContext) : IProcessedMessageStore
{
    public Task<bool> TryStartProcessingAsync(
        Guid eventId,
        string consumerName,
        CancellationToken cancellationToken) =>
        dbContext.TryStartProcessingAsync(eventId, consumerName, cancellationToken);
}
