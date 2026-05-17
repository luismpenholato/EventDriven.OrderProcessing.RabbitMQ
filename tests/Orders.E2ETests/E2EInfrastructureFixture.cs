using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Orders.Api.Controllers;
using Orders.Infrastructure;
using RabbitMQ.Client;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Orders.E2ETests;

public sealed class E2EInfrastructureFixture : IAsyncLifetime
{
    private const string RabbitMqUsername = "e2e";
    private const string RabbitMqPassword = "e2e";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:3.13-management-alpine")
        .WithUsername(RabbitMqUsername)
        .WithPassword(RabbitMqPassword)
        .Build();
    private readonly List<IHost> _workerHosts = [];
    private WebApplicationFactory<OrdersController> _apiFactory = null!;
    private IReadOnlyDictionary<string, string?> _settings = null!;

    public string OrdersDbConnectionString => BuildDatabaseConnectionString("orders_db");

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _rabbitMq.StartAsync());

        await CreateDatabasesAsync();
        await WaitForRabbitMqAsync();

        _settings = BuildSettings();
        var configuration = BuildConfiguration(_settings);

        _apiFactory = new WebApplicationFactory<OrdersController>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("E2E");
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(_settings));
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
            });

        await _apiFactory.Services.MigrateOrdersDatabaseAsync();

        _workerHosts.Add(await E2EWorkerHosts.StartPaymentsWorkerAsync(configuration, CancellationToken.None));
        _workerHosts.Add(await E2EWorkerHosts.StartInventoryWorkerAsync(configuration, CancellationToken.None));
        _workerHosts.Add(await E2EWorkerHosts.StartNotificationsWorkerAsync(configuration, CancellationToken.None));

        await Task.Delay(TimeSpan.FromSeconds(5));

        using (var warmup = _apiFactory.CreateClient())
        {
            warmup.Timeout = TimeSpan.FromMinutes(3);
            _ = await warmup.GetAsync("/health");
        }

        await Task.Delay(TimeSpan.FromSeconds(5));
    }

    public HttpClient CreateClient()
    {
        var client = _apiFactory.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(3);
        return client;
    }

    public async Task DisposeAsync()
    {
        _apiFactory?.Dispose();

        foreach (var host in _workerHosts)
        {
            await host.StopAsync();
            host.Dispose();
        }

        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _rabbitMq.DisposeAsync().AsTask());
    }

    private async Task CreateDatabasesAsync()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        foreach (var database in new[] { "orders_db", "payments_db", "inventory_db", "notifications_db" })
        {
            await using var command = new NpgsqlCommand($"""CREATE DATABASE "{database}" """, connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    private IReadOnlyDictionary<string, string?> BuildSettings()
    {
        var rabbitMqPort = _rabbitMq.GetMappedPublicPort(5672);

        return new Dictionary<string, string?>
        {
            ["ConnectionStrings:OrdersDb"] = BuildDatabaseConnectionString("orders_db"),
            ["ConnectionStrings:PaymentsDb"] = BuildDatabaseConnectionString("payments_db"),
            ["ConnectionStrings:InventoryDb"] = BuildDatabaseConnectionString("inventory_db"),
            ["ConnectionStrings:NotificationsDb"] = BuildDatabaseConnectionString("notifications_db"),
            ["RabbitMq:Host"] = "127.0.0.1",
            ["RabbitMq:Port"] = rabbitMqPort.ToString(),
            ["RabbitMq:VirtualHost"] = "/",
            ["RabbitMq:Username"] = RabbitMqUsername,
            ["RabbitMq:Password"] = RabbitMqPassword,
            ["Outbox:BatchSize"] = "20",
            ["Outbox:PollIntervalSeconds"] = "1",
            ["Outbox:MaxRetryCount"] = "5",
            ["Payments:SimulateRejection"] = "false"
        };
    }

    private static IConfiguration BuildConfiguration(IReadOnlyDictionary<string, string?> settings) =>
        new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

    private async Task WaitForRabbitMqAsync()
    {
        var port = _rabbitMq.GetMappedPublicPort(5672);

        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "127.0.0.1",
                    Port = port,
                    UserName = RabbitMqUsername,
                    Password = RabbitMqPassword
                };

                await using var connection = await factory.CreateConnectionAsync();
                return;
            }
            catch (Exception) when (attempt < 29)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        throw new InvalidOperationException("RabbitMQ did not become ready for E2E tests.");
    }

    private string BuildDatabaseConnectionString(string database)
    {
        var builder = new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Database = database
        };

        return builder.ConnectionString;
    }
}
