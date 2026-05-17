using Microsoft.EntityFrameworkCore;
using Payments.Application.Abstractions;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence;

public sealed class PaymentRepository(PaymentsDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    public Task AddAsync(Payment payment, CancellationToken cancellationToken) =>
        dbContext.Payments.AddAsync(payment, cancellationToken).AsTask();

    public Task UpdateAsync(Payment payment, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(payment).State == EntityState.Detached)
            dbContext.Payments.Attach(payment);

        return Task.CompletedTask;
    }
}