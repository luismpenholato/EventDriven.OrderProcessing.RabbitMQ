using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public sealed class OrderStockRequestRepository(InventoryDbContext dbContext) : IOrderStockRequestRepository
{
    public Task<OrderStockRequest?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.OrderStockRequests.FirstOrDefaultAsync(r => r.OrderId == orderId, cancellationToken);

    public Task AddAsync(OrderStockRequest request, CancellationToken cancellationToken) =>
        dbContext.OrderStockRequests.AddAsync(request, cancellationToken).AsTask();

    public Task UpdateAsync(OrderStockRequest request, CancellationToken cancellationToken)
    {
        dbContext.OrderStockRequests.Update(request);
        return Task.CompletedTask;
    }
}
