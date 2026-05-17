using Payments.Domain.Entities;

namespace Payments.Application.Payments.Commands.ProcessOrderPayment;

public enum ProcessOrderPaymentOutcome
{
    Skipped,
    Approved,
    Rejected
}

public sealed record ProcessOrderPaymentResult(
    ProcessOrderPaymentOutcome Outcome,
    Payment? Payment = null);
