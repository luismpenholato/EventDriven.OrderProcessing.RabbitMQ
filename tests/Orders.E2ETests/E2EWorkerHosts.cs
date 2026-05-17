extern alias NotificationsWorker;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationsWorker::Notifications.Worker.Idempotency;
using NotificationsWorker::Notifications.Worker.Messaging.Consumers;
using NotificationsWorker::Notifications.Worker.Persistence;
using Payments.Infrastructure;
using StockReservedConsumer = NotificationsWorker::Notifications.Worker.Messaging.Consumers.StockReservedConsumer;

namespace Orders.E2ETests;

internal static class E2EWorkerHosts
{
    public static async Task<IHost> StartPaymentsWorkerAsync(
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddConfiguration(configuration);
        builder.Services.AddPaymentsInfrastructure(builder.Configuration);

        var host = builder.Build();
        await host.Services.MigratePaymentsDatabaseAsync();
        await host.StartAsync(cancellationToken);
        return host;
    }

    public static async Task<IHost> StartInventoryWorkerAsync(
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddConfiguration(configuration);
        builder.Services.AddInventoryInfrastructure(builder.Configuration);

        var host = builder.Build();
        await host.Services.MigrateAndSeedInventoryAsync();
        await host.StartAsync(cancellationToken);
        return host;
    }

    public static async Task<IHost> StartNotificationsWorkerAsync(
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddConfiguration(configuration);

        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(StockReservedConsumer).Assembly));
        builder.Services.AddCorrelationContext();
        builder.Services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationsDb")));
        builder.Services.AddScoped<IProcessedMessageStore, EfProcessedMessageStore>();
        builder.Services.AddRabbitMqMessaging(builder.Configuration, bus =>
        {
            bus.AddConsumer<StockReservedConsumer>();
        });

        var host = builder.Build();

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
            await db.Database.MigrateAsync(cancellationToken);
        }

        await host.StartAsync(cancellationToken);
        return host;
    }
}
