using BuildingBlocks.Contracts.Payments;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using Inventory.Application.Abstractions;
using Inventory.Application.Stock.Commands.ReserveStockForOrder;
using Inventory.Infrastructure.Persistence;
using MassTransit;
using MediatR;

namespace Inventory.Infrastructure.Messaging.Consumers;

public sealed class PaymentApprovedConsumer(
    IMediator mediator,
    IInventoryEventPublisher eventPublisher,
    IProcessedMessageStore processedMessageStore,
    InventoryDbContext dbContext,
    ICorrelationContext correlationContext,
    ILogger<PaymentApprovedConsumer> logger) : IConsumer<PaymentApprovedEvent>
{
    private const string ConsumerName = nameof(PaymentApprovedConsumer);

    public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        using var correlationScope = new ConsumerCorrelationScope(correlationContext, message);

        ReserveStockForOrderResult? result = null;

        await IdempotentConsumerExecutor.ExecuteAsync(
            dbContext,
            processedMessageStore,
            message.EventId,
            ConsumerName,
            nameof(PaymentApprovedEvent),
            async ct => result = await mediator.Send(
                new ReserveStockForOrderCommand(
                    message.OrderId,
                    message.Items.Select(i => new ReserveStockItem(i.ProductId, i.Quantity)).ToList()),
                ct),
            logger,
            cancellationToken,
            publishAfterCommit: async ct =>
            {
                if (result is null)
                    return;

                switch (result.Outcome)
                {
                    case ReserveStockOutcome.Reserved when result.ReservedItems is not null:
                        await eventPublisher.PublishStockReservedAsync(
                            result.OrderId,
                            result.ReservedItems,
                            ct);
                        break;
                    case ReserveStockOutcome.Failed when result.FailureReason is not null:
                        await eventPublisher.PublishStockReservationFailedAsync(
                            result.OrderId,
                            result.FailureReason,
                            ct);
                        break;
                }
            });
    }
}