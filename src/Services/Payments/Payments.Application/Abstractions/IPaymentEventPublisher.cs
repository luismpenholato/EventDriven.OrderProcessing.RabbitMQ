using BuildingBlocks.Contracts.Orders;
using Payments.Domain.Entities;

namespace Payments.Application.Abstractions;

public interface IPaymentEventPublisher
{
    Task PublishApprovedAsync(
        Payment payment,
        IReadOnlyList<OrderCreatedItem> items,
        CancellationToken cancellationToken);

    Task PublishRejectedAsync(Guid orderId, string reason, CancellationToken cancellationToken);
}
