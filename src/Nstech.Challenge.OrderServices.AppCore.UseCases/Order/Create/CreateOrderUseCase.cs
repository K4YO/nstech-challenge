using FluentValidation;
using Microsoft.Extensions.Logging;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create;

public class CreateOrderUseCase : UseCase<CreateOrderDto, CreateOrderResultDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderUseCase(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateOrderDto> validator,
        ILogger<CreateOrderUseCase> logger) : base(logger, validator)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    protected override UseCaseResult<CreateOrderResultDto> HandleValidationFailure(Dictionary<string, string[]> errors)
    {
        var failures = errors.SelectMany(kvp => kvp.Value.Select(msg => new FailureDetail(msg, kvp.Key))).ToList();
        return UseCaseResult<CreateOrderResultDto>.Unprocessable("Validation failed", failures);
    }

    protected override async Task<UseCaseResult<CreateOrderResultDto>> ExecuteValidatedAsync(
        CreateOrderDto input,
        CancellationToken cancellationToken)
    {
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
                    Logger.LogWarning("Product {ProductId} not found for order creation by customer {CustomerId}",
                        item.ProductId, input.CustomerId);
                    var failures = new List<FailureDetail>
                    {
                        new($"Product {item.ProductId} does not exist", "Products")
                    };
                    return UseCaseResult<CreateOrderResultDto>.Unprocessable(
                        $"Product {item.ProductId} not found",
                        failures);
                }

                if (!product.HasSufficientStock(item.Quantity))
                {
                    Logger.LogWarning("Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}",
                        item.ProductId, product.AvailableQuantity, item.Quantity);
                    var failures = new List<FailureDetail>
                    {
                        new($"Product {item.ProductId} has insufficient stock. Available: {product.AvailableQuantity}, Requested: {item.Quantity}", "Stock")
                    };
                    return UseCaseResult<CreateOrderResultDto>.Unprocessable(
                        $"Insufficient stock for product {product.Id}",
                        failures);
                }

                // Usar ValueResult para criar OrderItem
                var orderItemResult = OrderItem.Create(item.ProductId, product.UnitPrice, item.Quantity);
                if (!orderItemResult.IsSuccess)
                {
                    var failures = orderItemResult.Failures
                        .Select(f => new FailureDetail(f.Description, "Items"))
                        .ToList();
                    return UseCaseResult<CreateOrderResultDto>.Unprocessable("Invalid order item", failures);
                }

                orderItems.Add(orderItemResult.Value!);
            }

            // Usar ValueResult para criar Order
            var orderResult = AppCore.Domain.OrderAggregate.Order.Create(input.CustomerId, input.Currency, orderItems);
            if (!orderResult.IsSuccess)
            {
                var failures = orderResult.Failures
                    .Select(f => new FailureDetail(f.Description, f.Tag))
                    .ToList();
                return UseCaseResult<CreateOrderResultDto>.Unprocessable("Failed to create order", failures);
            }

            var order = orderResult.Value!;
            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Order {OrderId} created for customer {CustomerId} with {ItemCount} items, total {Total} {Currency}",
                order.Id, order.CustomerId, order.Items.Count, order.Total, order.Currency);

            var resultDto = new CreateOrderResultDto(order.Id, order.Status.ToString());
            return UseCaseResult<CreateOrderResultDto>.Created(resultDto, "Order created successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error creating order for customer {CustomerId}", input.CustomerId);
            return UseCaseResult<CreateOrderResultDto>.Failure($"An error occurred while creating the order: {ex.Message}");
        }
    }
}



