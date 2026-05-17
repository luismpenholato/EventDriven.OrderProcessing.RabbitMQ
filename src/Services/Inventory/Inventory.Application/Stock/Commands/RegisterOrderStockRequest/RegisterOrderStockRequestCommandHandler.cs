using MediatR;
using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;

namespace Inventory.Application.Stock.Commands.RegisterOrderStockRequest;

public sealed class RegisterOrderStockRequestCommandHandler(
    IOrderStockRequestRepository repository) : IRequestHandler<RegisterOrderStockRequestCommand>
{
    public async Task Handle(RegisterOrderStockRequestCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existing is not null)
            return;

        var lines = request.Items
            .Select(i => OrderStockRequestLine.Create(i.ProductId, i.Quantity))
            .ToList();

        var stockRequest = OrderStockRequest.Create(request.OrderId, lines);
        await repository.AddAsync(stockRequest, cancellationToken);
    }
}
