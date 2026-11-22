using FluentAssertions;
using Inventory.Domain.Entities;

namespace Inventory.UnitTests.Domain;

public sealed class ProductReleaseTests
{
    [Fact]
    public void Release_AfterReserve_RestoresAvailableQuantity()
    {
        var product = Product.Create("PROD-001", "Mouse", 10);
        product.Reserve(4);

        product.Release(4);

        product.AvailableQuantity.Should().Be(10);
    }

    [Fact]
    public void Release_WhenQuantityInvalid_ShouldThrow()
    {
        var product = Product.Create("PROD-001", "Mouse", 10);

        var act = () => product.Release(0);

        act.Should().Throw<Exception>();
    }
}
