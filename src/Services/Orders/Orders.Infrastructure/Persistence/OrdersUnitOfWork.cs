using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions;

namespace Orders.Infrastructure.Persistence;

public sealed class OrdersUnitOfWork(OrdersDbContext dbContext) : IOrdersUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
