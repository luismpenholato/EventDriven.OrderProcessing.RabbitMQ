using MediatR;

namespace Inventory.Application.Stock.Commands.RegisterOrderStockRequest;

public sealed record RegisterOrderStockRequestCommand(
    Guid OrderId,
    IReadOnlyList<RegisterOrderStockItem> Items) : IRequest;

public sealed record RegisterOrderStockItem(string ProductId, int Quantity);
