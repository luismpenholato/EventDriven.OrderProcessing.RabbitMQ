using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Orders.Dtos;

namespace Orders.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>
{
    public async Task<OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        return order is null ? null : OrderResponseDto.From(order);
    }
}
