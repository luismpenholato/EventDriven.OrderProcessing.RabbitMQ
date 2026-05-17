using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Npgsql;
using Orders.Api.Contracts;

namespace Orders.E2ETests;

[Collection(E2EInfrastructureCollection.Name)]
public sealed class OrderProcessingE2ETests(E2EInfrastructureFixture fixture)
{
    private static readonly TimeSpan CompletionTimeout = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    [Fact]
    public async Task CreateOrder_ShouldProcessFullFlowUntilCompleted()
    {
        using var client = fixture.CreateClient();

        var payload = new CreateOrderRequest(
            "customer-e2e",
            [new CreateOrderItemRequest("PROD-001", 1, 99.90m)]);

        var createResponse = await client.PostAsJsonAsync("/api/orders", payload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();
        created.Should().NotBeNull();
        created!.Status.Should().Be("Pending");

        OrderResponse completed;

        try
        {
            completed = await WaitForStatusAsync(client, created.Id, "Completed", CompletionTimeout);
        }
        catch (TimeoutException ex)
        {
            var outbox = await ReadOutboxDiagnosticsAsync(fixture.OrdersDbConnectionString, created.Id);
            throw new TimeoutException($"{ex.Message} Outbox: {outbox}", ex);
        }

        completed.Status.Should().Be("Completed");
        completed.FailureReason.Should().BeNull();
    }

    private static async Task<OrderResponse> WaitForStatusAsync(
        HttpClient client,
        Guid orderId,
        string expectedStatus,
        TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        string? lastStatus = null;

        while (DateTime.UtcNow < deadline)
        {
            var order = await client.GetFromJsonAsync<OrderResponse>($"/api/orders/{orderId}");
            order.Should().NotBeNull();
            lastStatus = order!.Status;

            if (order.Status == "Failed")
                throw new InvalidOperationException($"Order failed: {order.FailureReason}");

            if (order.Status == expectedStatus)
                return order;

            await Task.Delay(PollInterval);
        }

        throw new TimeoutException(
            $"Order {orderId} did not reach status '{expectedStatus}' within {timeout}. Last status: {lastStatus ?? "unknown"}.");
    }

    private static async Task<string> ReadOutboxDiagnosticsAsync(string connectionString, Guid orderId)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT COUNT(*) FILTER (WHERE "ProcessedOnUtc" IS NULL),
                   COUNT(*) FILTER (WHERE "ProcessedOnUtc" IS NOT NULL),
                   COUNT(*) FILTER (WHERE "Error" IS NOT NULL)
            FROM outbox_messages
            WHERE "Payload" LIKE @orderId
            """,
            connection);

        command.Parameters.AddWithValue("orderId", $"%{orderId}%");
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return "no outbox rows for order";

        return $"pending={reader.GetInt64(0)}, processed={reader.GetInt64(1)}, failed={reader.GetInt64(2)}";
    }
}
