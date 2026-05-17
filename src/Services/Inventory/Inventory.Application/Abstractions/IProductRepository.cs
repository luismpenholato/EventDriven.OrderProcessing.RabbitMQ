using Inventory.Domain.Entities;

namespace Inventory.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken);
    Task UpdateAsync(Product product, CancellationToken cancellationToken);
}
