using BuildingBlocks.Contracts.Inventory;
using BuildingBlocks.Observability.Correlation;
using Inventory.Application.Abstractions;
using MassTransit;

namespace Inventory.Infrastructure.Messaging;

public sealed class InventoryEventPublisher(
    IPublishEndpoint publishEndpoint,
    ICorrelationContext correlationContext) : IInventoryEventPublisher
{
    public Task PublishStockReservedAsync(
        Guid orderId,
        IReadOnlyList<(string ProductId, int Quantity)> items,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new StockReservedEvent
        {
            CorrelationId = correlationContext.GetRequiredGuid(),
            EventVersion = StockReservedEvent.CurrentVersion,
            OrderId = orderId,
            Items = items.Select(i => new ReservedStockItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        return publishEndpoint.Publish(integrationEvent, cancellationToken);
    }

    public Task PublishStockReservationFailedAsync(Guid orderId, string reason, CancellationToken cancellationToken)
    {
        var integrationEvent = new StockReservationFailedEvent
        {
            CorrelationId = correlationContext.GetRequiredGuid(),
            EventVersion = StockReservationFailedEvent.CurrentVersion,
            OrderId = orderId,
            Reason = reason
        };

        return publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
