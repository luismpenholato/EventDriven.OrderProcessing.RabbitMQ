namespace Orders.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    PaymentApproved = 1,
    StockReserved = 2,
    Completed = 3,
    Failed = 4
}
