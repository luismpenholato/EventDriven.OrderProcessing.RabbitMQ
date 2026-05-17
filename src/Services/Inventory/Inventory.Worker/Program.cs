using BuildingBlocks.Observability.HealthChecks;
using BuildingBlocks.Observability.Logging;
using Inventory.Infrastructure;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilogLogging("Inventory.Worker")
    .ConfigureServices((context, services) =>
    {
        services.AddInventoryInfrastructure(context.Configuration);
        services.AddHealthChecks()
            .AddPostgresHealthCheck(context.Configuration, "InventoryDb")
            .AddRabbitMqHealthCheck(context.Configuration);
    })
    .Build();

await host.Services.MigrateAndSeedInventoryAsync();
await host.RunAsync();
