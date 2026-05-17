using MediatR;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public sealed record MarkOrderFailedCommand(Guid OrderId, string Reason) : IRequest;
