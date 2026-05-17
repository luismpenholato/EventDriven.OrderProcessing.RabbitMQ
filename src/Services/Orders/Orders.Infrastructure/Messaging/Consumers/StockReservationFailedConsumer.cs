using BuildingBlocks.Contracts.Inventory;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using MassTransit;
using MediatR;
using Orders.Application.Orders.Commands.UpdateOrderStatus;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Messaging.Consumers;

public sealed class StockReservationFailedConsumer(
    IMediator mediator,
    IProcessedMessageStore processedMessageStore,
    OrdersDbContext dbContext,
    ICorrelationContext correlationContext,
    ILogger<StockReservationFailedConsumer> logger) : IConsumer<StockReservationFailedEvent>
{
    private const string ConsumerName = nameof(StockReservationFailedConsumer);

    public async Task Consume(ConsumeContext<StockReservationFailedEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        using var correlationScope = new ConsumerCorrelationScope(correlationContext, message);

        await IdempotentConsumerExecutor.ExecuteAsync(
            dbContext,
            processedMessageStore,
            message.EventId,
            ConsumerName,
            nameof(StockReservationFailedEvent),
            ct => mediator.Send(new MarkOrderFailedCommand(message.OrderId, message.Reason), ct),
            logger,
            cancellationToken);
    }
}
