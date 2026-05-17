namespace BuildingBlocks.Contracts.Inventory;

public sealed record StockReservedEvent : IntegrationEvent
{
    public const int CurrentVersion = IntegrationEventVersions.V1;

    public required Guid OrderId { get; init; }

    public required IReadOnlyList<ReservedStockItem> Items { get; init; }
}

public sealed record ReservedStockItem
{
    public required string ProductId { get; init; }

    public required int Quantity { get; init; }
}
