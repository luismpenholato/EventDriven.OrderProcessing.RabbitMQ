using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Inventory.Domain.Entities;

public sealed class Product : AggregateRoot
{
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int AvailableQuantity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Product(Guid id) : base(id) { }

    private Product() { }

    public static Product Create(string sku, string name, int availableQuantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        if (availableQuantity < 0)
            throw new DomainException("Available quantity cannot be negative.");

        var now = DateTime.UtcNow;
        return new Product(Guid.NewGuid())
        {
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            AvailableQuantity = availableQuantity,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (AvailableQuantity < quantity)
            throw new DomainException($"Insufficient stock for product {Sku}.");

        AvailableQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        AvailableQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
