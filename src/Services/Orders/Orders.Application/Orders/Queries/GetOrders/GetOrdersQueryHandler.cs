using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Orders.Dtos;

namespace Orders.Application.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersQuery, PagedOrdersResponseDto>
{
    public async Task<PagedOrdersResponseDto> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var (items, total) = await orderRepository.GetPagedAsync(page, pageSize, cancellationToken);

        return new PagedOrdersResponseDto(
            items.Select(OrderResponseDto.From).ToList(),
            page,
            pageSize,
            total);
    }
}
