using BuildingBlocks.Contracts.Inventory;
using BuildingBlocks.Contracts.Notifications;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using MassTransit;
using MediatR;
using Notifications.Worker.Application.SendOrderNotification;
using Notifications.Worker.Persistence;

namespace Notifications.Worker.Messaging.Consumers;

public sealed class StockReservedConsumer(
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    IProcessedMessageStore processedMessageStore,
    NotificationsDbContext dbContext,
    ICorrelationContext correlationContext,
    ILogger<StockReservedConsumer> logger) : IConsumer<StockReservedEvent>
{
    private const string ConsumerName = nameof(StockReservedConsumer);

    public async Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        using var correlationScope = new ConsumerCorrelationScope(correlationContext, message);

        await IdempotentConsumerExecutor.ExecuteAsync(
            dbContext,
            processedMessageStore,
            message.EventId,
            ConsumerName,
            nameof(StockReservedEvent),
            ct => mediator.Send(new SendOrderNotificationCommand(message.OrderId), ct),
            logger,
            cancellationToken,
            publishAfterCommit: ct => publishEndpoint.Publish(
                new NotificationSentEvent
                {
                    CorrelationId = correlationContext.GetRequiredGuid(),
                    EventVersion = NotificationSentEvent.CurrentVersion,
                    OrderId = message.OrderId,
                    Channel = "email",
                    Message = $"Your order {message.OrderId} has been processed successfully."
                },
                ct));
    }
}