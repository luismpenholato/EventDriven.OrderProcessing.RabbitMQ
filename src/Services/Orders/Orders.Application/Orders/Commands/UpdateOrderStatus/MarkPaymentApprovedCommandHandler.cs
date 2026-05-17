using BuildingBlocks.Domain.Exceptions;
using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Orders;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public sealed class MarkPaymentApprovedCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<MarkPaymentApprovedCommand>
{
    public async Task Handle(MarkPaymentApprovedCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new DomainException($"Order {request.OrderId} not found.");

        OrderStatusProjection.AdvanceToPaymentApproved(order);
        await orderRepository.UpdateAsync(order, cancellationToken);
    }
}