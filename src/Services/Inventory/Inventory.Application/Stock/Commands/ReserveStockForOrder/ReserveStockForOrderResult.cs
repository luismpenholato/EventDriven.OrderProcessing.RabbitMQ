namespace Inventory.Application.Stock.Commands.ReserveStockForOrder;

public enum ReserveStockOutcome
{
    Skipped,
    Reserved,
    Failed
}

public sealed record ReserveStockForOrderResult(
    ReserveStockOutcome Outcome,
    Guid OrderId,
    IReadOnlyList<(string ProductId, int Quantity)>? ReservedItems = null,
    string? FailureReason = null);
