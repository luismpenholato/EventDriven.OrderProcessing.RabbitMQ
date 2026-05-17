using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.Persistence;

public static class InventoryDataSeeder
{
    public static async Task SeedAsync(InventoryDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Products.AnyAsync(cancellationToken))
            return;

        var products = new[]
        {
            Product.Create("PROD-001", "Wireless Mouse", 100),
            Product.Create("PROD-002", "Mechanical Keyboard", 50),
            Product.Create("PROD-003", "USB-C Hub", 75)
        };

        await dbContext.Products.AddRangeAsync(products, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Inventory seed completed with {Count} products.", products.Length);
    }
}
