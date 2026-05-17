namespace BuildingBlocks.Contracts.Inventory;

public sealed record StockReservationFailedEvent : IntegrationEvent
{
    public const int CurrentVersion = IntegrationEventVersions.V1;

    public required Guid OrderId { get; init; }

    public required string Reason { get; init; }
}
