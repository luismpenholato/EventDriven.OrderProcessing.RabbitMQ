using BuildingBlocks.Observability.HealthChecks;
using BuildingBlocks.Observability.Logging;
using Payments.Infrastructure;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilogLogging("Payments.Worker")
    .ConfigureServices((context, services) =>
    {
        services.AddPaymentsInfrastructure(context.Configuration);
        services.AddHealthChecks()
            .AddPostgresHealthCheck(context.Configuration, "PaymentsDb")
            .AddRabbitMqHealthCheck(context.Configuration);
    })
    .Build();

await host.Services.MigratePaymentsDatabaseAsync();
await host.RunAsync();
