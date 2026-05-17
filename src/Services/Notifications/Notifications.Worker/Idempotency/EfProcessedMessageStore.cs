using BuildingBlocks.Messaging.Idempotency;
using Notifications.Worker.Persistence;

namespace Notifications.Worker.Idempotency;

public sealed class EfProcessedMessageStore(NotificationsDbContext dbContext) : IProcessedMessageStore
{
    public Task<bool> TryStartProcessingAsync(
        Guid eventId,
        string consumerName,
        CancellationToken cancellationToken) =>
        dbContext.TryStartProcessingAsync(eventId, consumerName, cancellationToken);
}
