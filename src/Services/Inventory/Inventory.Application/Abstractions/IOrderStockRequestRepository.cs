using Inventory.Domain.Entities;

namespace Inventory.Application.Abstractions;

public interface IOrderStockRequestRepository
{
    Task<OrderStockRequest?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task AddAsync(OrderStockRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(OrderStockRequest request, CancellationToken cancellationToken);
}
