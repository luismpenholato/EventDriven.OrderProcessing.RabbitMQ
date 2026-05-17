using BuildingBlocks.Contracts.Orders;
using BuildingBlocks.Messaging.Idempotency;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Payments.Application.Abstractions;
using Payments.Application.Payments.Commands.ProcessOrderPayment;
using Payments.Domain.Entities;
using Payments.Infrastructure.Idempotency;
using Payments.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Payments.Infrastructure.Tests;

public sealed class ProcessOrderPaymentCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private PaymentsDbContext _dbContext = null!;
    private EfProcessedMessageStore _processedMessageStore = null!;
    private RecordingPaymentEventPublisher _publisher = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new PaymentsDbContext(options);
        _processedMessageStore = new EfProcessedMessageStore(_dbContext);
        _publisher = new RecordingPaymentEventPublisher();
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSameOrderIsProcessedTwice_OnlyCreatesOnePayment()
    {
        var orderId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        const string consumerName = "OrderCreatedConsumer";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Payments:SimulateRejection"] = "false"
            })
            .Build();

        var command = new ProcessOrderPaymentCommand(
            orderId,
            99.90m,
            "customer-1",
            [new OrderCreatedItem { ProductId = "PROD-001", Quantity = 1, UnitPrice = 99.90m }]);

        await ProcessOrderCreatedAsync(eventId, consumerName, command, configuration);
        await ProcessOrderCreatedAsync(Guid.NewGuid(), consumerName, command, configuration);

        var paymentCount = await _dbContext.Payments.CountAsync(p => p.OrderId == orderId);
        paymentCount.Should().Be(1);
        _publisher.ApprovedCount.Should().Be(1);
    }

    private async Task ProcessOrderCreatedAsync(
        Guid eventId,
        string consumerName,
        ProcessOrderPaymentCommand command,
        IConfiguration configuration)
    {
        var repository = new PaymentRepository(_dbContext);
        var handler = new ProcessOrderPaymentCommandHandler(
            repository,
            configuration,
            NullLogger<ProcessOrderPaymentCommandHandler>.Instance);

        ProcessOrderPaymentResult? result = null;

        await IdempotentConsumerExecutor.ExecuteAsync(
            _dbContext,
            _processedMessageStore,
            eventId,
            consumerName,
            "OrderCreatedEvent",
            async ct => result = await handler.Handle(command, ct),
            NullLogger.Instance,
            CancellationToken.None,
            publishAfterCommit: async ct =>
            {
                if (result?.Outcome == ProcessOrderPaymentOutcome.Approved && result.Payment is not null)
                    await _publisher.PublishApprovedAsync(result.Payment, command.Items, ct);
            });
    }

    private sealed class RecordingPaymentEventPublisher : IPaymentEventPublisher
    {
        public int ApprovedCount { get; private set; }

        public Task PublishApprovedAsync(
            Payment payment,
            IReadOnlyList<OrderCreatedItem> items,
            CancellationToken cancellationToken)
        {
            ApprovedCount++;
            return Task.CompletedTask;
        }

        public Task PublishRejectedAsync(Guid orderId, string reason, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}