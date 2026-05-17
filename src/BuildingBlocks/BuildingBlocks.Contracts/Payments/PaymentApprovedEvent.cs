using BuildingBlocks.Contracts.Orders;

namespace BuildingBlocks.Contracts.Payments;

public sealed record PaymentApprovedEvent : IntegrationEvent
{
    public const int CurrentVersion = IntegrationEventVersions.V1;

    public required Guid OrderId { get; init; }

    public required string PaymentId { get; init; }

    public required decimal Amount { get; init; }

    public required IReadOnlyList<OrderCreatedItem> Items { get; init; }
}
