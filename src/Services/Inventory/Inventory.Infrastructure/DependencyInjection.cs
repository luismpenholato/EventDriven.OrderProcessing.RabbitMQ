using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using Inventory.Application;
using Inventory.Application.Abstractions;
using Inventory.Infrastructure.Idempotency;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Messaging.Consumers;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInventoryApplication();
        services.AddCorrelationContext();

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("InventoryDb")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderStockRequestRepository, OrderStockRequestRepository>();
        services.AddScoped<IInventoryEventPublisher, InventoryEventPublisher>();
        services.AddScoped<IProcessedMessageStore, EfProcessedMessageStore>();

        services.AddRabbitMqMessaging(configuration, bus =>
        {
            bus.AddConsumer<OrderCreatedConsumer>();
            bus.AddConsumer<PaymentApprovedConsumer>();
        });

        return services;
    }

    public static async Task MigrateAndSeedInventoryAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Inventory.Seed");
        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();
        await InventoryDataSeeder.SeedAsync(db, logger);
    }
}
