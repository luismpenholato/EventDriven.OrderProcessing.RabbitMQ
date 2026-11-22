using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Idempotency;

public static class IdempotentConsumerExecutor
{
    public static Task ExecuteAsync(
        DbContext dbContext,
        IProcessedMessageStore processedMessageStore,
        Guid eventId,
        string consumerName,
        string eventName,
        Func<CancellationToken, Task> process,
        ILogger logger,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            dbContext,
            processedMessageStore,
            eventId,
            consumerName,
            eventName,
            process,
            logger,
            cancellationToken,
            publishAfterCommit: null);

    public static async Task ExecuteAsync(
        DbContext dbContext,
        IProcessedMessageStore processedMessageStore,
        Guid eventId,
        string consumerName,
        string eventName,
        Func<CancellationToken, Task> process,
        ILogger logger,
        CancellationToken cancellationToken,
        Func<CancellationToken, Task>? publishAfterCommit)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            if (!await processedMessageStore.TryStartProcessingAsync(eventId, consumerName, cancellationToken))
            {
                logger.LogInformation(
                    "Skipping duplicate {Event} {EventId}; ensuring post-commit publish",
                    eventName,
                    eventId);
                await transaction.CommitAsync(cancellationToken);
                await PublishAfterCommitAsync(publishAfterCommit, cancellationToken);
                return;
            }

            await process(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await PublishAfterCommitAsync(publishAfterCommit, cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();

            if (await IsAlreadyProcessedAsync(dbContext, eventId, consumerName, cancellationToken))
            {
                logger.LogInformation(
                    ex,
                    "Duplicate {Event} {EventId} detected after concurrency conflict; ensuring post-commit publish",
                    eventName,
                    eventId);
                await PublishAfterCommitAsync(publishAfterCommit, cancellationToken);
                return;
            }

            throw;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    private static Task PublishAfterCommitAsync(
        Func<CancellationToken, Task>? publishAfterCommit,
        CancellationToken cancellationToken) =>
        publishAfterCommit is null ? Task.CompletedTask : publishAfterCommit(cancellationToken);

    private static Task<bool> IsAlreadyProcessedAsync(
        DbContext dbContext,
        Guid eventId,
        string consumerName,
        CancellationToken cancellationToken) =>
        dbContext.Set<ProcessedMessage>()
            .AnyAsync(m => m.EventId == eventId && m.ConsumerName == consumerName, cancellationToken);
}