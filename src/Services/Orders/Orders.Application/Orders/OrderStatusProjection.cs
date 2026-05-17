using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Application.Orders;

internal static class OrderStatusProjection
{
    public static void AdvanceToPaymentApproved(Order order)
    {
        if (order.Status is OrderStatus.PaymentApproved or OrderStatus.StockReserved or OrderStatus.Completed)
            return;

        if (order.Status == OrderStatus.Failed)
            return;

        order.MarkPaymentApproved();
    }

    public static void AdvanceToStockReserved(Order order)
    {
        if (order.Status is OrderStatus.StockReserved or OrderStatus.Completed)
            return;

        if (order.Status == OrderStatus.Failed)
            return;

        AdvanceToPaymentApproved(order);

        if (order.Status == OrderStatus.PaymentApproved)
            order.MarkStockReserved();
    }

    public static void AdvanceToCompleted(Order order)
    {
        if (order.Status == OrderStatus.Completed)
            return;

        if (order.Status == OrderStatus.Failed)
            return;

        AdvanceToStockReserved(order);

        if (order.Status == OrderStatus.StockReserved)
            order.MarkCompleted();
    }
}
