using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Messaging.Outbox;

public sealed class OutboxDispatcherBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<OutboxOptions> options,
    ILogger<OutboxDispatcherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox dispatcher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while dispatching outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds), stoppingToken);
        }

        logger.LogInformation("Outbox dispatcher stopped");
    }

    private async Task DispatchPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxMessageStore>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var batchSize = options.Value.BatchSize;

        var pendingMessages = await outboxStore.GetPendingAsync(batchSize, cancellationToken);

        foreach (var message in pendingMessages)
        {
            try
            {
                var integrationEvent = OutboxMessageSerializer.Deserialize(message);
                await publishEndpoint.Publish(integrationEvent, cancellationToken);
                await outboxStore.MarkAsProcessedAsync(message.Id, cancellationToken);

                logger.LogInformation("Outbox message {MessageId} published", message.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                await outboxStore.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }
}
