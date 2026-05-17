using BuildingBlocks.Domain.Exceptions;
using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Orders;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public sealed class MarkOrderCompletedCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<MarkOrderCompletedCommand>
{
    public async Task Handle(MarkOrderCompletedCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new DomainException($"Order {request.OrderId} not found.");

        OrderStatusProjection.AdvanceToCompleted(order);
        await orderRepository.UpdateAsync(order, cancellationToken);
    }
}