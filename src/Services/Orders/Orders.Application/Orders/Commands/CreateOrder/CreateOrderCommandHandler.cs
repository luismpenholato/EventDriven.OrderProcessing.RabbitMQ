using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Orders.Dtos;
using Orders.Domain.Entities;

namespace Orders.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IOrderEventPublisher eventPublisher,
    IOrdersUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, OrderResponseDto>
{
    public async Task<OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var items = request.Items
            .Select(i => OrderItem.Create(i.ProductId, i.Quantity, i.UnitPrice))
            .ToList();

        var order = Order.Create(request.CustomerId, items);

        await orderRepository.AddAsync(order, cancellationToken);
        await eventPublisher.PublishOrderCreatedAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderResponseDto.From(order);
    }
}
