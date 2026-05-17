using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Orders.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.EventType).HasMaxLength(500).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.OccurredOnUtc).IsRequired();
        builder.Property(message => message.ProcessedOnUtc);
        builder.Property(message => message.RetryCount).IsRequired();
        builder.Property(message => message.Error).HasMaxLength(2000);

        builder.HasIndex(message => new { message.ProcessedOnUtc, message.OccurredOnUtc });
    }
}
