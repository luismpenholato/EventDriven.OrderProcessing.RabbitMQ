using FluentAssertions;
using Orders.Application.Orders;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.UnitTests.Application;

public sealed class OrderStatusProjectionTests
{
    [Fact]
    public void AdvanceToStockReserved_WhenPending_ShouldCatchUpToStockReserved()
    {
        var order = Order.Create("customer-1", [OrderItem.Create("PROD-001", 1, 10m)]);

        OrderStatusProjection.AdvanceToStockReserved(order);

        order.Status.Should().Be(OrderStatus.StockReserved);
    }

    [Fact]
    public void AdvanceToCompleted_WhenPending_ShouldCatchUpToCompleted()
    {
        var order = Order.Create("customer-1", [OrderItem.Create("PROD-001", 1, 10m)]);

        OrderStatusProjection.AdvanceToCompleted(order);

        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void AdvanceToCompleted_WhenPaymentApproved_ShouldCatchUpToCompleted()
    {
        var order = Order.Create("customer-1", [OrderItem.Create("PROD-001", 1, 10m)]);
        order.MarkPaymentApproved();

        OrderStatusProjection.AdvanceToCompleted(order);

        order.Status.Should().Be(OrderStatus.Completed);
    }
}
