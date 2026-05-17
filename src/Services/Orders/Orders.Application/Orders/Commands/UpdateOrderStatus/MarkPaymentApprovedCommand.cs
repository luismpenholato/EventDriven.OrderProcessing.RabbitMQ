using MediatR;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public sealed record MarkPaymentApprovedCommand(Guid OrderId) : IRequest;
