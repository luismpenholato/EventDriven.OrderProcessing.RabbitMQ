namespace Inventory.Application.Abstractions;

public interface IInventoryEventPublisher
{
    Task PublishStockReservedAsync(Guid orderId, IReadOnlyList<(string ProductId, int Quantity)> items, CancellationToken cancellationToken);
    Task PublishStockReservationFailedAsync(Guid orderId, string reason, CancellationToken cancellationToken);
}
