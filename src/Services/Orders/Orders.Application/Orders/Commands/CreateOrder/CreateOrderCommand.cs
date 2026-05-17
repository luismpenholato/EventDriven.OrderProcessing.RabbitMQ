using MediatR;
using Orders.Application.Orders.Dtos;

namespace Orders.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    string CustomerId,
    IReadOnlyList<CreateOrderItemDto> Items) : IRequest<OrderResponseDto>;

public sealed record CreateOrderItemDto(string ProductId, int Quantity, decimal UnitPrice);
