using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orders.Application;
using Orders.Application.Abstractions;
using Orders.Infrastructure.Idempotency;
using Orders.Infrastructure.Messaging;
using Orders.Infrastructure.Messaging.Consumers;
using Orders.Infrastructure.Outbox;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddOrdersApplication();

        if (environment.IsEnvironment("Testing"))
        {
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseInMemoryDatabase("orders-tests"));
        }
        else
        {
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("OrdersDb")));
        }

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrdersUnitOfWork, OrdersUnitOfWork>();
        services.AddScoped<IProcessedMessageStore, EfProcessedMessageStore>();
        services.AddScoped<IOutboxMessageStore, EfOutboxMessageStore>();

        if (environment.IsEnvironment("Testing"))
        {
            services.AddScoped<IOrderEventPublisher, NoOpOrderEventPublisher>();
        }
        else
        {
            services.AddScoped<IOrderEventPublisher, OrderEventPublisher>();
            services.AddOutboxDispatcher(configuration);
            services.AddRabbitMqMessaging(configuration, bus =>
            {
                bus.AddConsumer<PaymentApprovedConsumer>();
                bus.AddConsumer<PaymentRejectedConsumer>();
                bus.AddConsumer<StockReservedConsumer>();
                bus.AddConsumer<StockReservationFailedConsumer>();
                bus.AddConsumer<NotificationSentConsumer>();
            });
        }

        return services;
    }

    public static async Task MigrateOrdersDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();
    }
}
