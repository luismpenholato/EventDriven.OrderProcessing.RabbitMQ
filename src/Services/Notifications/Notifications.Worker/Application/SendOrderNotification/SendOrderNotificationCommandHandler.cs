using MediatR;
using Microsoft.Extensions.Logging;

namespace Notifications.Worker.Application.SendOrderNotification;

public sealed class SendOrderNotificationCommandHandler(
    ILogger<SendOrderNotificationCommandHandler> logger) : IRequestHandler<SendOrderNotificationCommand>
{
    public Task Handle(SendOrderNotificationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Notification recorded for order {OrderId}", request.OrderId);
        return Task.CompletedTask;
    }
}