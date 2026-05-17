using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Observability.Correlation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application;
using Payments.Application.Abstractions;
using Payments.Infrastructure.Idempotency;
using Payments.Infrastructure.Messaging;
using Payments.Infrastructure.Messaging.Consumers;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPaymentsApplication();
        services.AddCorrelationContext();

        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PaymentsDb")));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentEventPublisher, PaymentEventPublisher>();
        services.AddScoped<IProcessedMessageStore, EfProcessedMessageStore>();

        services.AddRabbitMqMessaging(configuration, bus =>
        {
            bus.AddConsumer<OrderCreatedConsumer>();
        });

        return services;
    }

    public static async Task MigratePaymentsDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();
    }
}
