using BuildingBlocks.Contracts.Orders;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Payments.Application.Abstractions;
using Payments.Application.Payments.Commands.ProcessOrderPayment;
using Payments.Domain.Entities;

namespace Payments.UnitTests.Application;

public sealed class ProcessOrderPaymentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenPaymentAlreadyExists_DoesNotCreateAnotherPayment()
    {
        var orderId = Guid.NewGuid();
        var repository = new InMemoryPaymentRepository();
        var existing = Payment.Create(orderId, 50m);
        existing.Approve();
        repository.Seed(existing);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Payments:SimulateRejection"] = "false" })
            .Build();

        var handler = new ProcessOrderPaymentCommandHandler(
            repository,
            configuration,
            NullLogger<ProcessOrderPaymentCommandHandler>.Instance);

        var result = await handler.Handle(
            new ProcessOrderPaymentCommand(
                orderId,
                50m,
                "customer-1",
                [new OrderCreatedItem { ProductId = "PROD-001", Quantity = 1, UnitPrice = 50m }]),
            CancellationToken.None);

        repository.Payments.Should().HaveCount(1);
        result.Outcome.Should().Be(ProcessOrderPaymentOutcome.Skipped);
    }

    private sealed class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly List<Payment> _payments = [];

        public IReadOnlyList<Payment> Payments => _payments;

        public void Seed(Payment payment) => _payments.Add(payment);

        public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
            Task.FromResult(_payments.FirstOrDefault(p => p.OrderId == orderId));

        public Task AddAsync(Payment payment, CancellationToken cancellationToken)
        {
            _payments.Add(payment);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Payment payment, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}