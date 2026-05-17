using MediatR;

namespace Notifications.Worker.Application.SendOrderNotification;

public sealed record SendOrderNotificationCommand(Guid OrderId) : IRequest;
