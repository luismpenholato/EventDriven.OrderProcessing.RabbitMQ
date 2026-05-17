using FluentAssertions;
using Orders.Application.Orders.Commands.CreateOrder;

namespace Orders.UnitTests.Application;

public sealed class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenCustomerIdIsEmpty_ShouldFail()
    {
        var command = new CreateOrderCommand(string.Empty, [new CreateOrderItemDto("PROD-001", 1, 10m)]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenCommandIsValid_ShouldPass()
    {
        var command = new CreateOrderCommand("customer-1", [new CreateOrderItemDto("PROD-001", 1, 10m)]);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}
