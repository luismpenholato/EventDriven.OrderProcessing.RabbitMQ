namespace BuildingBlocks.Contracts.Notifications;

public sealed record NotificationSentEvent : IntegrationEvent
{
    public const int CurrentVersion = IntegrationEventVersions.V1;

    public required Guid OrderId { get; init; }

    public required string Channel { get; init; }

    public required string Message { get; init; }
}
