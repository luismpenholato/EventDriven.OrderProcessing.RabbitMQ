using Orders.Application.Abstractions;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Messaging;

public sealed class NoOpOrderEventPublisher : IOrderEventPublisher
{
    public Task PublishOrderCreatedAsync(Order order, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
