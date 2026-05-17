using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Orders.Api.Contracts;

namespace Orders.E2ETests;

/// <summary>
/// E2E opcional contra stack Docker Compose já em execução.
/// Ative com: set RUN_DOCKER_E2E=true (Windows) ou export RUN_DOCKER_E2E=true (Linux/macOS).
/// Pré-requisito: docker compose up -d --build
/// </summary>
public sealed class DockerComposeOrderProcessingE2ETests
{
    private static readonly Uri ApiBaseAddress = new("http://localhost:8080");
    private static readonly TimeSpan CompletionTimeout = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    [Fact]
    public async Task CreateOrder_WhenDockerComposeIsRunning_ShouldReachCompleted()
    {
        if (!IsDockerE2ERunRequested())
            return;

        using var client = new HttpClient { BaseAddress = ApiBaseAddress };

        (await IsOrdersApiHealthyAsync(client))
            .Should()
            .BeTrue("execute 'docker compose up -d --build' antes de rodar este teste");

        var payload = new CreateOrderRequest(
            "customer-docker-e2e",
            [new CreateOrderItemRequest("PROD-001", 1, 49.90m)]);

        var createResponse = await client.PostAsJsonAsync("/api/orders", payload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();
        created.Should().NotBeNull();

        var completed = await WaitForStatusAsync(client, created!.Id, "Completed", CompletionTimeout);
        completed.Status.Should().Be("Completed");
    }

    private static bool IsDockerE2ERunRequested() =>
        string.Equals(Environment.GetEnvironmentVariable("RUN_DOCKER_E2E"), "true", StringComparison.OrdinalIgnoreCase);

    private static async Task<bool> IsOrdersApiHealthyAsync(HttpClient client)
    {
        try
        {
            var response = await client.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    private static async Task<OrderResponse> WaitForStatusAsync(
        HttpClient client,
        Guid orderId,
        string expectedStatus,
        TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);

        while (!cts.IsCancellationRequested)
        {
            var order = await client.GetFromJsonAsync<OrderResponse>($"/api/orders/{orderId}", cts.Token);
            order.Should().NotBeNull();

            if (order!.Status == "Failed")
                throw new InvalidOperationException($"Order failed: {order.FailureReason}");

            if (order.Status == expectedStatus)
                return order;

            await Task.Delay(PollInterval, cts.Token);
        }

        throw new TimeoutException($"Order {orderId} did not reach status '{expectedStatus}' within {timeout}.");
    }
}
