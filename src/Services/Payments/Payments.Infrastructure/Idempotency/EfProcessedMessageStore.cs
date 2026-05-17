using BuildingBlocks.Messaging.Idempotency;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Idempotency;

public sealed class EfProcessedMessageStore(PaymentsDbContext dbContext) : IProcessedMessageStore
{
    public Task<bool> TryStartProcessingAsync(
        Guid eventId,
        string consumerName,
        CancellationToken cancellationToken) =>
        dbContext.TryStartProcessingAsync(eventId, consumerName, cancellationToken);
}
