using BuildingBlocks.Contracts.Payments;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using MassTransit;
using MediatR;
using Orders.Application.Orders.Commands.UpdateOrderStatus;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Messaging.Consumers;

public sealed class PaymentApprovedConsumer(
    IMediator mediator,
    IProcessedMessageStore processedMessageStore,
    OrdersDbContext dbContext,
    ICorrelationContext correlationContext,
    ILogger<PaymentApprovedConsumer> logger) : IConsumer<PaymentApprovedEvent>
{
    private const string ConsumerName = nameof(PaymentApprovedConsumer);

    public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        using var correlationScope = new ConsumerCorrelationScope(correlationContext, message);

        await IdempotentConsumerExecutor.ExecuteAsync(
            dbContext,
            processedMessageStore,
            message.EventId,
            ConsumerName,
            nameof(PaymentApprovedEvent),
            ct => mediator.Send(new MarkPaymentApprovedCommand(message.OrderId), ct),
            logger,
            cancellationToken);
    }
}
