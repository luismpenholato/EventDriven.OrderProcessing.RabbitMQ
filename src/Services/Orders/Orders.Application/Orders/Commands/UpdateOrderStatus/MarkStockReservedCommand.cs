using MediatR;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public sealed record MarkStockReservedCommand(Guid OrderId) : IRequest;
