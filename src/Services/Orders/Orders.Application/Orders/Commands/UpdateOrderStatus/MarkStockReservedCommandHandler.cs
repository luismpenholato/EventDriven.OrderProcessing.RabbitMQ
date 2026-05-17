using BuildingBlocks.Domain.Exceptions;
using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Orders;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public sealed class MarkStockReservedCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<MarkStockReservedCommand>
{
    public async Task Handle(MarkStockReservedCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new DomainException($"Order {request.OrderId} not found.");

        OrderStatusProjection.AdvanceToStockReserved(order);
        await orderRepository.UpdateAsync(order, cancellationToken);
    }
}