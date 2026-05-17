using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public sealed class ProductRepository(InventoryDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken) =>
        dbContext.Products.FirstOrDefaultAsync(
            p => p.Sku == sku.Trim().ToUpperInvariant(),
            cancellationToken);

    public Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        dbContext.Products.Update(product);
        return Task.CompletedTask;
    }
}
