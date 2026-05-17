using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Messaging.Idempotency;

public sealed class ProcessedMessageEntityConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("processed_messages");
        builder.HasKey(message => new { message.EventId, message.ConsumerName });
        builder.Property(message => message.ConsumerName).HasMaxLength(200).IsRequired();
        builder.Property(message => message.ProcessedAt).IsRequired();
    }
}
