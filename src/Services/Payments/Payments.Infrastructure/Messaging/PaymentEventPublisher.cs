using BuildingBlocks.Contracts.Orders;
using BuildingBlocks.Contracts.Payments;
using BuildingBlocks.Observability.Correlation;
using MassTransit;
using Payments.Application.Abstractions;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Messaging;

public sealed class PaymentEventPublisher(
    IPublishEndpoint publishEndpoint,
    ICorrelationContext correlationContext) : IPaymentEventPublisher
{
    public Task PublishApprovedAsync(
        Payment payment,
        IReadOnlyList<OrderCreatedItem> items,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new PaymentApprovedEvent
        {
            CorrelationId = correlationContext.GetRequiredGuid(),
            EventVersion = PaymentApprovedEvent.CurrentVersion,
            OrderId = payment.OrderId,
            PaymentId = payment.Id.ToString(),
            Amount = payment.Amount,
            Items = items
        };

        return publishEndpoint.Publish(integrationEvent, cancellationToken);
    }

    public Task PublishRejectedAsync(Guid orderId, string reason, CancellationToken cancellationToken)
    {
        var integrationEvent = new PaymentRejectedEvent
        {
            CorrelationId = correlationContext.GetRequiredGuid(),
            EventVersion = PaymentRejectedEvent.CurrentVersion,
            OrderId = orderId,
            Reason = reason
        };

        return publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
