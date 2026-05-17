namespace BuildingBlocks.Contracts.Orders;

public sealed record OrderCreatedEvent : IntegrationEvent
{
    public const int CurrentVersion = IntegrationEventVersions.V1;

    public required Guid OrderId { get; init; }

    public required string CustomerId { get; init; }

    public required decimal TotalAmount { get; init; }

    public required IReadOnlyList<OrderCreatedItem> Items { get; init; }
}

public sealed record OrderCreatedItem
{
    public required string ProductId { get; init; }

    public required int Quantity { get; init; }

    public required decimal UnitPrice { get; init; }
}
