using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Outbox;

public sealed class EfOutboxMessageStore(
    OrdersDbContext dbContext,
    IOptions<OutboxOptions> options) : IOutboxMessageStore
{
    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken) =>
        dbContext.OutboxMessages.AddAsync(message, cancellationToken).AsTask();

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        var maxRetryCount = options.Value.MaxRetryCount;

        return await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null && message.RetryCount < maxRetryCount)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken)
            ?? throw new InvalidOperationException($"Outbox message '{messageId}' was not found.");

        message.ProcessedOnUtc = DateTime.UtcNow;
        message.Error = null;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken)
            ?? throw new InvalidOperationException($"Outbox message '{messageId}' was not found.");

        message.RetryCount++;
        message.Error = TruncateError(error);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string TruncateError(string error) =>
        error.Length <= 2000 ? error : error[..2000];
}
