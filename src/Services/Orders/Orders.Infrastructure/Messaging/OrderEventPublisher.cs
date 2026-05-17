using BuildingBlocks.Contracts.Orders;
using BuildingBlocks.Messaging.Outbox;
using BuildingBlocks.Observability.Correlation;
using Orders.Application.Abstractions;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Messaging;

public sealed class OrderEventPublisher(
    IOutboxMessageStore outboxMessageStore,
    ICorrelationContext correlationContext) : IOrderEventPublisher
{
    public Task PublishOrderCreatedAsync(Order order, CancellationToken cancellationToken)
    {
        var integrationEvent = new OrderCreatedEvent
        {
            CorrelationId = correlationContext.GetRequiredGuid(),
            EventVersion = OrderCreatedEvent.CurrentVersion,
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderCreatedItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        var outboxMessage = OutboxMessageSerializer.FromIntegrationEvent(integrationEvent);
        return outboxMessageStore.AddAsync(outboxMessage, cancellationToken);
    }
}
