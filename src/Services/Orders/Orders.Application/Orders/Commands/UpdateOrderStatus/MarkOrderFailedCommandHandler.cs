using BuildingBlocks.Domain.Exceptions;
using MediatR;
using Orders.Application.Abstractions;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public sealed class MarkOrderFailedCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<MarkOrderFailedCommand>
{
    public async Task Handle(MarkOrderFailedCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new DomainException($"Order {request.OrderId} not found.");

        order.MarkFailed(request.Reason);
        await orderRepository.UpdateAsync(order, cancellationToken);
    }
}
