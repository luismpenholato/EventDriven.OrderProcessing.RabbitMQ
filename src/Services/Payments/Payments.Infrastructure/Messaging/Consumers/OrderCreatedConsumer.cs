using BuildingBlocks.Contracts.Orders;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Abstractions;
using Payments.Application.Payments.Commands.ProcessOrderPayment;
using Payments.Domain.Entities;
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

        try
        {
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
                publishAfterCommit: ct => PublishOutcomeAsync(result, message, ct));
        }
        catch (DbUpdateException ex) when (PostgresUniqueViolations.IsPaymentOrderIdUniqueViolation(ex))
        {
            // Concurrent OrderCreated (distinct EventId) raced on payments.OrderId unique index.
            dbContext.ChangeTracker.Clear();

            var existing = await dbContext.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderId == message.OrderId, cancellationToken);

            if (existing is null)
                throw;

            logger.LogInformation(
                ex,
                "Recovered payment OrderId unique race for order {OrderId}; treating as skipped",
                message.OrderId);

            result = new ProcessOrderPaymentResult(ProcessOrderPaymentOutcome.Skipped, existing);

            await IdempotentConsumerExecutor.ExecuteAsync(
                dbContext,
                processedMessageStore,
                message.EventId,
                ConsumerName,
                nameof(OrderCreatedEvent),
                _ => Task.CompletedTask,
                logger,
                cancellationToken,
                publishAfterCommit: ct => PublishExistingPaymentAsync(existing, message, ct));
        }
    }

    private Task PublishOutcomeAsync(
        ProcessOrderPaymentResult? result,
        OrderCreatedEvent message,
        CancellationToken cancellationToken)
    {
        if (result is null)
            return Task.CompletedTask;

        return result.Outcome switch
        {
            ProcessOrderPaymentOutcome.Approved when result.Payment is not null =>
                eventPublisher.PublishApprovedAsync(result.Payment, message.Items, cancellationToken),
            ProcessOrderPaymentOutcome.Rejected =>
                eventPublisher.PublishRejectedAsync(
                    message.OrderId,
                    result.Payment?.RejectionReason ?? "Payment rejected.",
                    cancellationToken),
            _ => Task.CompletedTask
        };
    }

    private Task PublishExistingPaymentAsync(
        Payment payment,
        OrderCreatedEvent message,
        CancellationToken cancellationToken) =>
        payment.Status switch
        {
            PaymentStatus.Approved =>
                eventPublisher.PublishApprovedAsync(payment, message.Items, cancellationToken),
            PaymentStatus.Rejected =>
                eventPublisher.PublishRejectedAsync(
                    payment.OrderId,
                    payment.RejectionReason ?? "Payment rejected.",
                    cancellationToken),
            _ => Task.CompletedTask
        };
}
