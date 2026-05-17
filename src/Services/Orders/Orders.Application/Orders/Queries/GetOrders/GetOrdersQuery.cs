using MediatR;
using Orders.Application.Orders.Dtos;

namespace Orders.Application.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(int Page = 1, int PageSize = 10) : IRequest<PagedOrdersResponseDto>;
