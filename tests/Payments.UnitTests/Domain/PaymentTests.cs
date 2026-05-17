using FluentAssertions;
using Payments.Domain.Entities;

namespace Payments.UnitTests.Domain;

public sealed class PaymentTests
{
    [Fact]
    public void Approve_ShouldSetStatusToApproved()
    {
        var payment = Payment.Create(Guid.NewGuid(), 100m);

        payment.Approve();

        payment.Status.Should().Be(PaymentStatus.Approved);
    }

    [Fact]
    public void Reject_ShouldSetStatusToRejected()
    {
        var payment = Payment.Create(Guid.NewGuid(), 100m);

        payment.Reject("Insufficient funds");

        payment.Status.Should().Be(PaymentStatus.Rejected);
        payment.RejectionReason.Should().Be("Insufficient funds");
    }
}
