using Orders.Domain.Entities;

namespace Orders.Application.Abstractions;

public interface IOrderEventPublisher
{
    Task PublishOrderCreatedAsync(Order order, CancellationToken cancellationToken);
}
