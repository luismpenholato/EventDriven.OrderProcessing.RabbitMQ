using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Inventory.Domain.Entities;

public sealed class OrderStockRequestLine
{
    public string ProductId { get; private set; } = string.Empty;
    public int Quantity { get; private set; }

    public static OrderStockRequestLine Create(string productId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new DomainException("ProductId is required.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        return new OrderStockRequestLine
        {
            ProductId = productId.Trim(),
            Quantity = quantity
        };
    }
}

public sealed class OrderStockRequest : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public bool IsReserved { get; private set; }
    public IReadOnlyList<OrderStockRequestLine> Lines { get; private set; } = [];

    private OrderStockRequest(Guid id) : base(id) { }

    private OrderStockRequest() { }

    public static OrderStockRequest Create(Guid orderId, IEnumerable<OrderStockRequestLine> lines)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("OrderId is required.");

        var lineList = lines.ToList();
        if (lineList.Count == 0)
            throw new DomainException("At least one line is required.");

        return new OrderStockRequest(Guid.NewGuid())
        {
            OrderId = orderId,
            Lines = lineList,
            IsReserved = false
        };
    }

    public void MarkReserved() => IsReserved = true;
}
