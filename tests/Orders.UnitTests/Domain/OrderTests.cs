using FluentAssertions;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.UnitTests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void Create_ShouldInitializeOrderAsPending()
    {
        var order = Order.Create("customer-1", [OrderItem.Create("PROD-001", 2, 10m)]);

        order.Status.Should().Be(OrderStatus.Pending);
        order.TotalAmount.Should().Be(20m);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void MarkPaymentApproved_WhenPending_ShouldUpdateStatus()
    {
        var order = Order.Create("customer-1", [OrderItem.Create("PROD-001", 1, 5m)]);

        order.MarkPaymentApproved();

        order.Status.Should().Be(OrderStatus.PaymentApproved);
    }

    [Fact]
    public void MarkPaymentApproved_WhenAlreadyApproved_ShouldBeIdempotent()
    {
        var order = Order.Create("customer-1", [OrderItem.Create("PROD-001", 1, 5m)]);
        order.MarkPaymentApproved();

        var act = () => order.MarkPaymentApproved();

        act.Should().NotThrow();
        order.Status.Should().Be(OrderStatus.PaymentApproved);
    }

    [Fact]
    public void MarkCompleted_ShouldFollowHappyPath()
    {
        var order = Order.Create("customer-1", [OrderItem.Create("PROD-001", 1, 5m)]);

        order.MarkPaymentApproved();
        order.MarkStockReserved();
        order.MarkCompleted();

        order.Status.Should().Be(OrderStatus.Completed);
    }
}
