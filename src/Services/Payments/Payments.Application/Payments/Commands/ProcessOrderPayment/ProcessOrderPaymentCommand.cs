using BuildingBlocks.Contracts.Orders;
using MediatR;

namespace Payments.Application.Payments.Commands.ProcessOrderPayment;

public sealed record ProcessOrderPaymentCommand(
    Guid OrderId,
    decimal Amount,
    string CustomerId,
    IReadOnlyList<OrderCreatedItem> Items) : IRequest<ProcessOrderPaymentResult>;
