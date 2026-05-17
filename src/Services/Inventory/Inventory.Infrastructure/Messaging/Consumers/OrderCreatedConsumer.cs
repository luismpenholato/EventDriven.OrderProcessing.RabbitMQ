using BuildingBlocks.Contracts.Orders;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using Inventory.Application.Stock.Commands.RegisterOrderStockRequest;
using Inventory.Infrastructure.Persistence;
using MassTransit;
using MediatR;

namespace Inventory.Infrastructure.Messaging.Consumers;

public sealed class OrderCreatedConsumer(
    IMediator mediator,
    IProcessedMessageStore processedMessageStore,
    InventoryDbContext dbContext,
    ICorrelationContext correlationContext,
    ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    private const string ConsumerName = nameof(OrderCreatedConsumer);

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        using var correlationScope = new ConsumerCorrelationScope(correlationContext, message);

        await IdempotentConsumerExecutor.ExecuteAsync(
            dbContext,
            processedMessageStore,
            message.EventId,
            ConsumerName,
            nameof(OrderCreatedEvent),
            ct => mediator.Send(
                new RegisterOrderStockRequestCommand(
                    message.OrderId,
                    message.Items.Select(i => new RegisterOrderStockItem(i.ProductId, i.Quantity)).ToList()),
                ct),
            logger,
            cancellationToken);
    }
}
