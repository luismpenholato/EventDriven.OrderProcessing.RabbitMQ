using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace BuildingBlocks.Observability.HealthChecks;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddPostgresHealthCheck(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        string connectionStringName = "OrdersDb")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            builder.AddNpgSql(connectionString, name: "postgresql");
        }

        return builder;
    }

    public static IHealthChecksBuilder AddRabbitMqHealthCheck(
        this IHealthChecksBuilder builder,
        IConfiguration configuration)
    {
        var host = configuration["RabbitMq:Host"] ?? "localhost";
        var username = configuration["RabbitMq:Username"] ?? "guest";
        var password = configuration["RabbitMq:Password"] ?? "guest";

        builder.AddRabbitMQ(
            _ =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = host,
                    UserName = username,
                    Password = password
                };

                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            },
            name: "rabbitmq");

        return builder;
    }
}
