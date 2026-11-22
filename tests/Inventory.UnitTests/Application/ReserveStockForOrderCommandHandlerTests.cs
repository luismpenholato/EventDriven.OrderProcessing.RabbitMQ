using FluentAssertions;
using Inventory.Application.Abstractions;
using Inventory.Application.Stock.Commands.ReserveStockForOrder;
using Inventory.Domain.Entities;

namespace Inventory.UnitTests.Application;

public sealed class ReserveStockForOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLaterSkuFails_ReleasesEarlierReservations()
    {
        var orderId = Guid.NewGuid();
        var productRepo = new InMemoryProductRepository();
        productRepo.Seed(Product.Create("SKU-A", "Item A", 10));
        productRepo.Seed(Product.Create("SKU-B", "Item B", 1));

        var handler = new ReserveStockForOrderCommandHandler(productRepo, new InMemoryOrderStockRequestRepository());

        var result = await handler.Handle(
            new ReserveStockForOrderCommand(
                orderId,
                [
                    new ReserveStockItem("SKU-A", 3),
                    new ReserveStockItem("SKU-B", 5)
                ]),
            CancellationToken.None);

        result.Outcome.Should().Be(ReserveStockOutcome.Failed);
        productRepo.GetRequired("SKU-A").AvailableQuantity.Should().Be(10);
        productRepo.GetRequired("SKU-B").AvailableQuantity.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenLaterSkuMissing_ReleasesEarlierReservations()
    {
        var orderId = Guid.NewGuid();
        var productRepo = new InMemoryProductRepository();
        productRepo.Seed(Product.Create("SKU-A", "Item A", 10));

        var handler = new ReserveStockForOrderCommandHandler(productRepo, new InMemoryOrderStockRequestRepository());

        var result = await handler.Handle(
            new ReserveStockForOrderCommand(
                orderId,
                [
                    new ReserveStockItem("SKU-A", 2),
                    new ReserveStockItem("SKU-MISSING", 1)
                ]),
            CancellationToken.None);

        result.Outcome.Should().Be(ReserveStockOutcome.Failed);
        result.FailureReason.Should().Contain("SKU-MISSING");
        productRepo.GetRequired("SKU-A").AvailableQuantity.Should().Be(10);
    }

    private sealed class InMemoryProductRepository : IProductRepository
    {
        private readonly Dictionary<string, Product> _products = new(StringComparer.OrdinalIgnoreCase);

        public void Seed(Product product) => _products[product.Sku] = product;

        public Product GetRequired(string sku) => _products[sku];

        public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken) =>
            Task.FromResult(_products.TryGetValue(sku, out var product) ? product : null);

        public Task UpdateAsync(Product product, CancellationToken cancellationToken)
        {
            _products[product.Sku] = product;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryOrderStockRequestRepository : IOrderStockRequestRepository
    {
        public Task<OrderStockRequest?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
            Task.FromResult<OrderStockRequest?>(null);

        public Task AddAsync(OrderStockRequest request, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task UpdateAsync(OrderStockRequest request, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
