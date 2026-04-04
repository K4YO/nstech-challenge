using FluentValidation;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel;

public class CancelOrderUseCase : UseCase<CancelOrderDto, UseCaseResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CancelOrderDto> _validator;

    public CancelOrderUseCase(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IValidator<CancelOrderDto> validator)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _validator = validator;
    }

    public override async Task<UseCaseResult> ExecuteAsync(
        CancelOrderDto input,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return UseCaseResult.Failure("Validation failed", errors);
        }

        try
        {
            var order = await _orderRepository.GetByIdAsync(input.OrderId, cancellationToken);
            if (order == null)
            {
                return UseCaseResult.Failure(
                    "Order not found",
                    new Dictionary<string, string[]>
                    {
                        ["Order"] = new[] { $"Order {input.OrderId} does not exist" }
                    });
            }

            if (order.IsCanceled())
            {
                return UseCaseResult.Success("Order is already canceled");
            }

            // Usar ValueResult do domain
            var cancelResult = order.Cancel();
            if (!cancelResult.IsSuccess)
            {
                var errorMessages = cancelResult.Failures
                    .ToDictionary(f => f.Tag ?? "Order", f => new[] { f.Description });
                return UseCaseResult.Failure("Cannot cancel order", errorMessages);
            }

            if (order.IsConfirmed())
            {
                var productIds = order.Items.Select(x => x.ProductId).Distinct().ToList();
                var products = (await _productRepository.GetByIdsAsync(productIds, cancellationToken))
                    .ToDictionary(p => p.Id);

                foreach (var item in order.Items)
                {
                    if (products.TryGetValue(item.ProductId, out var product))
                    {
                        var releaseResult = product.ReleaseReservedStock(item.Quantity);
                        if (!releaseResult.IsSuccess)
                        {
                            var errorMessages = releaseResult.Failures
                                .ToDictionary(f => f.Tag ?? "Stock", f => new[] { f.Description });
                            return UseCaseResult.Failure("Cannot release reserved stock", errorMessages);
                        }

                        await _productRepository.UpdateAsync(product, cancellationToken);
                    }
                }
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);

            return UseCaseResult.Success("Order canceled successfully");
        }
        catch (Exception ex)
        {
            return UseCaseResult.Failure($"An error occurred while canceling the order: {ex.Message}");
        }
    }
}


