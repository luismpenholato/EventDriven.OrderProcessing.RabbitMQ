using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Payments.Domain.Entities;

public enum PaymentStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public sealed class Payment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Payment(Guid id) : base(id) { }

    private Payment() { }

    public static Payment Create(Guid orderId, decimal amount)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("OrderId is required.");

        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        var now = DateTime.UtcNow;
        return new Payment(Guid.NewGuid())
        {
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Approve()
    {
        if (Status == PaymentStatus.Approved)
            return;

        if (Status == PaymentStatus.Rejected)
            throw new DomainException("Cannot approve a rejected payment.");

        Status = PaymentStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status == PaymentStatus.Rejected)
            return;

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason is required.");

        Status = PaymentStatus.Rejected;
        RejectionReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
