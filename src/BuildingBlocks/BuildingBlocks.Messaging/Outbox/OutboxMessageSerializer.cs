using System.Text.Json;
using BuildingBlocks.Contracts;

namespace BuildingBlocks.Messaging.Outbox;

public static class OutboxMessageSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static OutboxMessage FromIntegrationEvent<T>(T integrationEvent) where T : IntegrationEvent
    {
        var eventType = typeof(T);

        return new OutboxMessage
        {
            Id = integrationEvent.EventId,
            EventType = eventType.AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(integrationEvent, eventType, SerializerOptions),
            OccurredOnUtc = integrationEvent.OccurredOnUtc,
            RetryCount = 0
        };
    }

    public static object Deserialize(OutboxMessage message)
    {
        var eventType = Type.GetType(message.EventType)
            ?? throw new InvalidOperationException($"Unable to resolve event type '{message.EventType}'.");

        return JsonSerializer.Deserialize(message.Payload, eventType, SerializerOptions)
            ?? throw new InvalidOperationException($"Unable to deserialize outbox message '{message.Id}'.");
    }
}
