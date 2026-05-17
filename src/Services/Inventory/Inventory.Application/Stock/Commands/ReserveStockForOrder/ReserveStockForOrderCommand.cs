using MediatR;

namespace Inventory.Application.Stock.Commands.ReserveStockForOrder;

public sealed record ReserveStockItem(string ProductId, int Quantity);

public sealed record ReserveStockForOrderCommand(
    Guid OrderId,
    IReadOnlyList<ReserveStockItem>? Items = null) : IRequest<ReserveStockForOrderResult>;
