using Orders.Application.Orders.Dtos;

namespace Orders.Api.Contracts;

public sealed record CreateOrderRequest(string CustomerId, IReadOnlyList<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(string ProductId, int Quantity, decimal UnitPrice);

public sealed record OrderResponse(
    Guid Id,
    string CustomerId,
    string Status,
    decimal TotalAmount,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<OrderItemResponse> Items)
{
    public static OrderResponse From(OrderResponseDto dto) => new(
        dto.Id,
        dto.CustomerId,
        dto.Status,
        dto.TotalAmount,
        dto.FailureReason,
        dto.CreatedAt,
        dto.UpdatedAt,
        dto.Items.Select(i => new OrderItemResponse(i.ProductId, i.Quantity, i.UnitPrice, i.LineTotal)).ToList());
}

public sealed record OrderItemResponse(string ProductId, int Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record PagedOrdersResponse(
    IReadOnlyList<OrderResponse> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public static PagedOrdersResponse From(PagedOrdersResponseDto dto) => new(
        dto.Items.Select(OrderResponse.From).ToList(),
        dto.Page,
        dto.PageSize,
        dto.TotalCount);
}
