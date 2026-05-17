using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.Persistence;

namespace Orders.IntegrationTests;

public sealed class CreateOrderEndpointTests(OrdersApiFactory factory) : IClassFixture<OrdersApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostOrders_WhenPayloadIsValid_ShouldReturnCreated()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await db.Database.EnsureCreatedAsync();

        var payload = new
        {
            customerId = "customer-integration",
            items = new[]
            {
                new { productId = "PROD-001", quantity = 1, unitPrice = 99.90m }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/orders", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
