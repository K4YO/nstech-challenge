using FluentValidation;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm;

public class ConfirmOrderUseCase : UseCase<ConfirmOrderDto, UseCaseResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<ConfirmOrderDto> _validator;

    public ConfirmOrderUseCase(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IValidator<ConfirmOrderDto> validator)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _validator = validator;
    }

    public override async Task<UseCaseResult> ExecuteAsync(
        ConfirmOrderDto input,
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

            if (order.IsConfirmed())
            {
                return UseCaseResult.Success("Order is already confirmed");
            }

            var confirmResult = order.Confirm();
            if (!confirmResult.IsSuccess)
            {
                var errorMessages = confirmResult.Failures
                    .ToDictionary(f => f.Tag ?? "Order", f => new[] { f.Description });
                return UseCaseResult.Failure("Cannot confirm order", errorMessages);
            }

            var productIds = order.Items.Select(x => x.ProductId).Distinct().ToList();
            var products = (await _productRepository.GetByIdsAsync(productIds, cancellationToken))
                .ToDictionary(p => p.Id);

            foreach (var item in order.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    return UseCaseResult.Failure(
                        $"Product {item.ProductId} not found",
                        new Dictionary<string, string[]>
                        {
                            ["Products"] = new[] { $"Product {item.ProductId} does not exist" }
                        });
                }

                var reserveResult = product.ReserveStock(item.Quantity);
                if (!reserveResult.IsSuccess)
                {
                    var errorMessages = reserveResult.Failures
                        .ToDictionary(f => f.Tag ?? "Stock", f => new[] { f.Description });
                    return UseCaseResult.Failure("Cannot reserve stock", errorMessages);
                }

                await _productRepository.UpdateAsync(product, cancellationToken);
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);

            return UseCaseResult.Success("Order confirmed successfully");
        }
        catch (Exception ex)
        {
            return UseCaseResult.Failure($"An error occurred while confirming the order: {ex.Message}");
        }
    }
}
