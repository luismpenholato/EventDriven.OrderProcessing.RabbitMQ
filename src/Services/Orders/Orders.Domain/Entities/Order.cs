using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using Orders.Domain.Enums;

namespace Orders.Domain.Entities;

public sealed class Order : AggregateRoot
{
    public string CustomerId { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order(Guid id) : base(id) { }

    private Order() { }

    public static Order Create(string customerId, IEnumerable<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new DomainException("CustomerId is required.");

        var itemList = items.ToList();
        if (itemList.Count == 0)
            throw new DomainException("Order must contain at least one item.");

        var now = DateTime.UtcNow;
        var order = new Order(Guid.NewGuid())
        {
            CustomerId = customerId.Trim(),
            Status = OrderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        order._items.AddRange(itemList);
        order.TotalAmount = order._items.Sum(i => i.LineTotal);

        return order;
    }

    public void MarkPaymentApproved()
    {
        if (Status is OrderStatus.PaymentApproved or OrderStatus.StockReserved or OrderStatus.Completed)
            return;

        EnsureStatus(OrderStatus.Pending, "payment approval");
        Status = OrderStatus.PaymentApproved;
        Touch();
    }

    public void MarkStockReserved()
    {
        if (Status is OrderStatus.StockReserved or OrderStatus.Completed)
            return;

        EnsureStatus(OrderStatus.PaymentApproved, "stock reservation");
        Status = OrderStatus.StockReserved;
        Touch();
    }

    public void MarkCompleted()
    {
        if (Status == OrderStatus.Completed)
            return;

        EnsureStatus(OrderStatus.StockReserved, "completion");
        Status = OrderStatus.Completed;
        Touch();
    }

    public void MarkFailed(string reason)
    {
        if (Status is OrderStatus.Completed or OrderStatus.Failed)
            return;

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Failure reason is required.");

        Status = OrderStatus.Failed;
        FailureReason = reason.Trim();
        Touch();
    }

    private void EnsureStatus(OrderStatus expected, string action)
    {
        if (Status != expected)
            throw new DomainException($"Cannot apply {action} when order is {Status}.");
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
