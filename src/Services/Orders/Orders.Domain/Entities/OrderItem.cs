using BuildingBlocks.Domain.Abstractions;

namespace Orders.Domain.Entities;

public sealed class OrderItem : Entity
{
    public string ProductId { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;

    private OrderItem(Guid id) : base(id) { }

    private OrderItem() { }

    public static OrderItem Create(string productId, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ProductId is required.", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new OrderItem(Guid.NewGuid())
        {
            ProductId = productId.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
