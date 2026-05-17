using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Application.Orders.Dtos;

public sealed record OrderResponseDto(
    Guid Id,
    string CustomerId,
    string Status,
    decimal TotalAmount,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<OrderItemResponseDto> Items)
{
    public static OrderResponseDto From(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status.ToString(),
        order.TotalAmount,
        order.FailureReason,
        order.CreatedAt,
        order.UpdatedAt,
        order.Items.Select(i => new OrderItemResponseDto(i.ProductId, i.Quantity, i.UnitPrice, i.LineTotal)).ToList());
}

public sealed record OrderItemResponseDto(
    string ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record PagedOrdersResponseDto(
    IReadOnlyList<OrderResponseDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
