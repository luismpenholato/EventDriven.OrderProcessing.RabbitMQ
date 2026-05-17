using BuildingBlocks.Contracts.Payments;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using MassTransit;
using MediatR;
using Orders.Application.Orders.Commands.UpdateOrderStatus;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Messaging.Consumers;

public sealed class PaymentRejectedConsumer(
    IMediator mediator,
    IProcessedMessageStore processedMessageStore,
    OrdersDbContext dbContext,
    ICorrelationContext correlationContext,
    ILogger<PaymentRejectedConsumer> logger) : IConsumer<PaymentRejectedEvent>
{
    private const string ConsumerName = nameof(PaymentRejectedConsumer);

    public async Task Consume(ConsumeContext<PaymentRejectedEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        using var correlationScope = new ConsumerCorrelationScope(correlationContext, message);

        await IdempotentConsumerExecutor.ExecuteAsync(
            dbContext,
            processedMessageStore,
            message.EventId,
            ConsumerName,
            nameof(PaymentRejectedEvent),
            ct => mediator.Send(new MarkOrderFailedCommand(message.OrderId, message.Reason), ct),
            logger,
            cancellationToken);
    }
}
