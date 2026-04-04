using FluentValidation;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create;

public class CreateOrderUseCase : UseCase<CreateOrderDto, UseCaseResult<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateOrderDto> _validator;

    public CreateOrderUseCase(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IValidator<CreateOrderDto> validator)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _validator = validator;
    }

    public override async Task<UseCaseResult<Guid>> ExecuteAsync(
        CreateOrderDto input,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return UseCaseResult<Guid>.Failure("Validation failed", errors);
        }

        try
        {
            var productIds = input.Items.Select(x => x.ProductId).Distinct().ToList();
            var products = (await _productRepository.GetByIdsAsync(productIds, cancellationToken))
                .ToDictionary(p => p.Id);

            var orderItems = new List<OrderItem>();

            foreach (var item in input.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    return UseCaseResult<Guid>.Failure(
                        $"Product {item.ProductId} not found",
                        new Dictionary<string, string[]>
                        {
                            ["Products"] = new[] { $"Product {item.ProductId} does not exist" }
                        });
                }

                if (!product.HasSufficientStock(item.Quantity))
                {
                    return UseCaseResult<Guid>.Failure(
                        $"Insufficient stock for product {product.Id}",
                        new Dictionary<string, string[]>
                        {
                            ["Stock"] = new[] { $"Product {item.ProductId} has insufficient stock. Available: {product.AvailableQuantity}, Requested: {item.Quantity}" }
                        });
                }

                // Usar ValueResult para criar OrderItem
                var orderItemResult = OrderItem.Create(item.ProductId, product.UnitPrice, item.Quantity);
                if (!orderItemResult.IsSuccess)
                {
                    var errorMessages = orderItemResult.Failures
                        .Select(f => f.Description)
                        .ToArray();
                    return UseCaseResult<Guid>.Failure("Invalid order item", new Dictionary<string, string[]> { ["Items"] = errorMessages });
                }

                orderItems.Add(orderItemResult.Value!);
            }

            // Usar ValueResult para criar Order
            var orderResult = AppCore.Domain.OrderAggregate.Order.Create(input.CustomerId, input.Currency, orderItems);
            if (!orderResult.IsSuccess)
            {
                var errorMessages = orderResult.Failures
                    .ToDictionary(f => f.Tag ?? "Order", f => new[] { f.Description });
                return UseCaseResult<Guid>.Failure("Failed to create order", errorMessages);
            }

            var order = orderResult.Value!;
            await _orderRepository.AddAsync(order, cancellationToken);

            return UseCaseResult<Guid>.Success(order.Id, "Order created successfully");
        }
        catch (Exception ex)
        {
            return UseCaseResult<Guid>.Failure($"An error occurred while creating the order: {ex.Message}");
        }
    }
}

