using BuildingBlocks.Messaging.Idempotency;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Payments.Infrastructure.Idempotency;
using Payments.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Payments.Infrastructure.Tests;

public sealed class IdempotentConsumerExecutorTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private PaymentsDbContext _dbContext = null!;
    private EfProcessedMessageStore _processedMessageStore = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new PaymentsDbContext(options);
        _processedMessageStore = new EfProcessedMessageStore(_dbContext);
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSameEventIsProcessedTwice_DoesNotThrowAndRunsOnce()
    {
        var eventId = Guid.NewGuid();
        const string consumerName = "OrderCreatedConsumer";
        var executions = 0;

        await RunAsync(eventId, consumerName, () =>
        {
            executions++;
            return Task.CompletedTask;
        });

        await RunAsync(eventId, consumerName, () =>
        {
            executions++;
            return Task.CompletedTask;
        });

        executions.Should().Be(1);

        var processedCount = await _dbContext.ProcessedMessages
            .CountAsync(m => m.EventId == eventId && m.ConsumerName == consumerName);

        processedCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDuplicateEvent_ReRunsPublishAfterCommit()
    {
        var eventId = Guid.NewGuid();
        const string consumerName = "OrderCreatedConsumer";
        var publishCount = 0;

        await IdempotentConsumerExecutor.ExecuteAsync(
            _dbContext,
            _processedMessageStore,
            eventId,
            consumerName,
            "OrderCreatedEvent",
            _ => Task.CompletedTask,
            NullLogger.Instance,
            CancellationToken.None,
            _ =>
            {
                publishCount++;
                return Task.CompletedTask;
            });

        await IdempotentConsumerExecutor.ExecuteAsync(
            _dbContext,
            _processedMessageStore,
            eventId,
            consumerName,
            "OrderCreatedEvent",
            _ => Task.CompletedTask,
            NullLogger.Instance,
            CancellationToken.None,
            _ =>
            {
                publishCount++;
                return Task.CompletedTask;
            });

        publishCount.Should().Be(2);
    }

    private Task RunAsync(Guid eventId, string consumerName, Func<Task> process) =>
        IdempotentConsumerExecutor.ExecuteAsync(
            _dbContext,
            _processedMessageStore,
            eventId,
            consumerName,
            "OrderCreatedEvent",
            async _ => await process(),
            NullLogger.Instance,
            CancellationToken.None);
}