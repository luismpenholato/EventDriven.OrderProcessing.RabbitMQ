using System.Text.Json;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public sealed class OrderStockRequestConfiguration : IEntityTypeConfiguration<OrderStockRequest>
{
    public void Configure(EntityTypeBuilder<OrderStockRequest> builder)
    {
        builder.ToTable("order_stock_requests");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.OrderId).IsUnique();
        builder.Property(r => r.IsReserved).IsRequired();
        builder.Property(r => r.Lines)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<OrderStockRequestLine>>(v, (JsonSerializerOptions?)null) ?? new List<OrderStockRequestLine>())
            .HasColumnType("jsonb");
    }
}
