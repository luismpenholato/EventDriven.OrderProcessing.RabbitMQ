using BuildingBlocks.Contracts.Inventory;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using MassTransit;
using MediatR;
using Orders.Application.Orders.Commands.UpdateOrderStatus;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Messaging.Consumers;

public sealed class StockReservedConsumer(
    IMediator mediator,
    IProcessedMessageStore processedMessageStore,
    OrdersDbContext dbContext,
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
            ct => mediator.Send(new MarkStockReservedCommand(message.OrderId), ct),
            logger,
            cancellationToken);
    }
}
