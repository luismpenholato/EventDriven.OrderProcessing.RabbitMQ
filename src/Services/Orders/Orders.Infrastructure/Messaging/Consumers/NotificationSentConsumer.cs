using BuildingBlocks.Contracts.Notifications;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using MassTransit;
using MediatR;
using Orders.Application.Orders.Commands.UpdateOrderStatus;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Messaging.Consumers;

public sealed class NotificationSentConsumer(
    IMediator mediator,
    IProcessedMessageStore processedMessageStore,
    OrdersDbContext dbContext,
    ICorrelationContext correlationContext,
    ILogger<NotificationSentConsumer> logger) : IConsumer<NotificationSentEvent>
{
    private const string ConsumerName = nameof(NotificationSentConsumer);

    public async Task Consume(ConsumeContext<NotificationSentEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        using var correlationScope = new ConsumerCorrelationScope(correlationContext, message);

        await IdempotentConsumerExecutor.ExecuteAsync(
            dbContext,
            processedMessageStore,
            message.EventId,
            ConsumerName,
            nameof(NotificationSentEvent),
            ct => mediator.Send(new MarkOrderCompletedCommand(message.OrderId), ct),
            logger,
            cancellationToken);
    }
}
