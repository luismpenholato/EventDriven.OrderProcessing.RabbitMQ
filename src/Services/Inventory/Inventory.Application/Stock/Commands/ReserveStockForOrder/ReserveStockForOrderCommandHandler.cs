using BuildingBlocks.Domain.Exceptions;
using Inventory.Application.Abstractions;
using MediatR;

namespace Inventory.Application.Stock.Commands.ReserveStockForOrder;

public sealed class ReserveStockForOrderCommandHandler(
    IProductRepository productRepository,
    IOrderStockRequestRepository orderStockRequestRepository) : IRequestHandler<ReserveStockForOrderCommand, ReserveStockForOrderResult>
{
    public async Task<ReserveStockForOrderResult> Handle(
        ReserveStockForOrderCommand request,
        CancellationToken cancellationToken)
    {
        var stockRequest = await orderStockRequestRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (stockRequest?.IsReserved == true)
            return new ReserveStockForOrderResult(ReserveStockOutcome.Skipped, request.OrderId);

        var lines = ResolveLines(request, stockRequest);
        if (lines.Count == 0)
        {
            return new ReserveStockForOrderResult(
                ReserveStockOutcome.Failed,
                request.OrderId,
                FailureReason: "No stock lines available to reserve.");
        }

        var reserved = new List<(string ProductId, int Quantity)>();

        try
        {
            foreach (var item in lines)
            {
                var product = await productRepository.GetBySkuAsync(item.ProductId, cancellationToken);
                if (product is null)
                {
                    await CompensateReservationsAsync(reserved, cancellationToken);
                    return new ReserveStockForOrderResult(
                        ReserveStockOutcome.Failed,
                        request.OrderId,
                        FailureReason: $"Product {item.ProductId} not found.");
                }

                product.Reserve(item.Quantity);
                await productRepository.UpdateAsync(product, cancellationToken);
                reserved.Add((item.ProductId, item.Quantity));
            }

            if (stockRequest is not null)
            {
                stockRequest.MarkReserved();
                await orderStockRequestRepository.UpdateAsync(stockRequest, cancellationToken);
            }

            return new ReserveStockForOrderResult(
                ReserveStockOutcome.Reserved,
                request.OrderId,
                reserved);
        }
        catch (DomainException ex)
        {
            await CompensateReservationsAsync(reserved, cancellationToken);
            return new ReserveStockForOrderResult(
                ReserveStockOutcome.Failed,
                request.OrderId,
                FailureReason: ex.Message);
        }
    }

    private async Task CompensateReservationsAsync(
        IReadOnlyList<(string ProductId, int Quantity)> reserved,
        CancellationToken cancellationToken)
    {
        foreach (var item in reserved)
        {
            var product = await productRepository.GetBySkuAsync(item.ProductId, cancellationToken);
            if (product is null)
                continue;

            product.Release(item.Quantity);
            await productRepository.UpdateAsync(product, cancellationToken);
        }
    }

    private static List<(string ProductId, int Quantity)> ResolveLines(
        ReserveStockForOrderCommand request,
        Domain.Entities.OrderStockRequest? stockRequest)
    {
        if (request.Items is { Count: > 0 })
        {
            return request.Items
                .Select(i => (i.ProductId, i.Quantity))
                .ToList();
        }

        return stockRequest?.Lines
            .Select(l => (l.ProductId, l.Quantity))
            .ToList() ?? [];
    }
}