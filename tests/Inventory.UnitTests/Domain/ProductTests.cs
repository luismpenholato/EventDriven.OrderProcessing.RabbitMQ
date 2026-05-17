using FluentAssertions;
using Inventory.Domain.Entities;

namespace Inventory.UnitTests.Domain;

public sealed class ProductTests
{
    [Fact]
    public void Reserve_WhenStockIsAvailable_ShouldDecreaseQuantity()
    {
        var product = Product.Create("PROD-001", "Mouse", 10);

        product.Reserve(3);

        product.AvailableQuantity.Should().Be(7);
    }

    [Fact]
    public void Reserve_WhenStockIsInsufficient_ShouldThrow()
    {
        var product = Product.Create("PROD-001", "Mouse", 2);

        var act = () => product.Reserve(5);

        act.Should().Throw<Exception>();
    }
}
