using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using BuildingBlocks.Observability.HealthChecks;
using BuildingBlocks.Observability.Logging;
using Microsoft.EntityFrameworkCore;
using Notifications.Worker.Idempotency;
using Notifications.Worker.Messaging.Consumers;
using Notifications.Worker.Persistence;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilogLogging("Notifications.Worker")
    .ConfigureServices((context, services) =>
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        services.AddCorrelationContext();

        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(context.Configuration.GetConnectionString("NotificationsDb")));

        services.AddScoped<IProcessedMessageStore, EfProcessedMessageStore>();

        services.AddRabbitMqMessaging(context.Configuration, bus =>
        {
            bus.AddConsumer<StockReservedConsumer>();
        });

        services.AddHealthChecks()
            .AddPostgresHealthCheck(context.Configuration, "NotificationsDb")
            .AddRabbitMqHealthCheck(context.Configuration);
    })
    .Build();

await MigrateNotificationsDatabaseAsync(host.Services);
await host.RunAsync();

static async Task MigrateNotificationsDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    await db.Database.MigrateAsync();
}
