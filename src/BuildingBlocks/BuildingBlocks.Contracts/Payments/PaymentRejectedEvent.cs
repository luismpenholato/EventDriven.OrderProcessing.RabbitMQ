namespace BuildingBlocks.Contracts.Payments;

public sealed record PaymentRejectedEvent : IntegrationEvent
{
    public const int CurrentVersion = IntegrationEventVersions.V1;

    public required Guid OrderId { get; init; }

    public required string Reason { get; init; }
}
