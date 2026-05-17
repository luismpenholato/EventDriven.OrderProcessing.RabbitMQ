using BuildingBlocks.Contracts.Orders;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using MassTransit;
using MediatR;
using Payments.Application.Abstractions;
using Payments.Application.Payments.Commands.ProcessOrderPayment;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Messaging.Consumers;

public sealed class OrderCreatedConsumer(
    IMediator mediator,
    IPaymentEventPublisher eventPublisher,
    IProcessedMessageStore processedMessageStore,
    PaymentsDbContext dbContext,
    ICorrelationContext correlationContext,
    ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    private const string ConsumerName = nameof(OrderCreatedConsumer);

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        using var correlationScope = new ConsumerCorrelationScope(correlationContext, message);

        ProcessOrderPaymentResult? result = null;

        await IdempotentConsumerExecutor.ExecuteAsync(
            dbContext,
            processedMessageStore,
            message.EventId,
            ConsumerName,
            nameof(OrderCreatedEvent),
            async ct => result = await mediator.Send(
                new ProcessOrderPaymentCommand(
                    message.OrderId,
                    message.TotalAmount,
                    message.CustomerId,
                    message.Items),
                ct),
            logger,
            cancellationToken,
            publishAfterCommit: async ct =>
            {
                if (result is null)
                    return;

                switch (result.Outcome)
                {
                    case ProcessOrderPaymentOutcome.Approved when result.Payment is not null:
                        await eventPublisher.PublishApprovedAsync(result.Payment, message.Items, ct);
                        break;
                    case ProcessOrderPaymentOutcome.Rejected:
                        await eventPublisher.PublishRejectedAsync(
                            message.OrderId,
                            result.Payment?.RejectionReason ?? "Payment rejected.",
                            ct);
                        break;
                }
            });
    }
}