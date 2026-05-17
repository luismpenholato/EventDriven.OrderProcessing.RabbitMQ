using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payments.Application.Abstractions;
using Payments.Domain.Entities;

namespace Payments.Application.Payments.Commands.ProcessOrderPayment;

public sealed class ProcessOrderPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IConfiguration configuration,
    ILogger<ProcessOrderPaymentCommandHandler> logger) : IRequestHandler<ProcessOrderPaymentCommand, ProcessOrderPaymentResult>
{
    public async Task<ProcessOrderPaymentResult> Handle(
        ProcessOrderPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existing is not null)
        {
            logger.LogInformation("Payment already exists for order {OrderId}, skipping", request.OrderId);
            return new ProcessOrderPaymentResult(ProcessOrderPaymentOutcome.Skipped);
        }

        var payment = Payment.Create(request.OrderId, request.Amount);
        await paymentRepository.AddAsync(payment, cancellationToken);

        var simulateRejection = configuration.GetValue<bool>("Payments:SimulateRejection");
        var rejectCustomerIds = configuration.GetSection("Payments:RejectCustomerIds").Get<string[]>() ?? [];

        if (simulateRejection || rejectCustomerIds.Contains(request.CustomerId, StringComparer.OrdinalIgnoreCase))
        {
            payment.Reject("Payment declined by simulated gateway.");
            return new ProcessOrderPaymentResult(ProcessOrderPaymentOutcome.Rejected, payment);
        }

        payment.Approve();
        return new ProcessOrderPaymentResult(ProcessOrderPaymentOutcome.Approved, payment);
    }
}