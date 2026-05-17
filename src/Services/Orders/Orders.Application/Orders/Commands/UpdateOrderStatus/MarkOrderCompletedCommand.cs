using MediatR;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public sealed record MarkOrderCompletedCommand(Guid OrderId) : IRequest;
