using MediatR;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Orders.Commands.CreateOrder;
using Orders.Application.Orders.Queries.GetOrderById;
using Orders.Application.Orders.Queries.GetOrders;
using Orders.Api.Contracts;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.Items.Select(i => new CreateOrderItemDto(i.ProductId, i.Quantity, i.UnitPrice)).ToList());

        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, OrderResponse.From(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(OrderResponse.From(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedOrdersResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetOrdersQuery(page, pageSize), cancellationToken);
        return Ok(PagedOrdersResponse.From(result));
    }
}
