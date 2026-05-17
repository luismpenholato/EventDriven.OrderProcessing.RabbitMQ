using BuildingBlocks.Messaging.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Worker.Persistence;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProcessedMessageEntityConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
