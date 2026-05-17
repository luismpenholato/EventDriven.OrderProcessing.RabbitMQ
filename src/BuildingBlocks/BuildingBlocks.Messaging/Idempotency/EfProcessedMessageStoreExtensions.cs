using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Messaging.Idempotency;

public static class EfProcessedMessageStoreExtensions
{
    public static async Task<bool> TryStartProcessingAsync(
        this DbContext dbContext,
        Guid eventId,
        string consumerName,
        CancellationToken cancellationToken)
    {
        var processedAt = DateTime.UtcNow;

        var rowsAffected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO processed_messages ("EventId", "ConsumerName", "ProcessedAt")
             VALUES ({eventId}, {consumerName}, {processedAt})
             ON CONFLICT ("EventId", "ConsumerName") DO NOTHING
             """,
            cancellationToken);

        return rowsAffected > 0;
    }
}
