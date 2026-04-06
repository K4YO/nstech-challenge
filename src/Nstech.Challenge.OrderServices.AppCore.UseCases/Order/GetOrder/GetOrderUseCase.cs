using FluentValidation;
using Microsoft.Extensions.Logging;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder;

public class GetOrderUseCase : UseCase<GetOrderDto, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderUseCase(
        IOrderRepository orderRepository,
        IValidator<GetOrderDto> validator,
        ILogger<GetOrderUseCase> logger) : base(logger, validator)
    {
        _orderRepository = orderRepository;
    }

    protected override UseCaseResult<OrderDto> HandleValidationFailure(Dictionary<string, string[]> errors)
    {
        var failures = errors.SelectMany(kvp => kvp.Value.Select(msg => new FailureDetail(msg, kvp.Key))).ToList();
        return UseCaseResult<OrderDto>.Unprocessable("Validation failed", failures);
    }

    protected override async Task<UseCaseResult<OrderDto>> ExecuteValidatedAsync(
        GetOrderDto input,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(input.OrderId, cancellationToken);
            if (order == null)
            {
                Logger.LogWarning("Order {OrderId} not found", input.OrderId);
                var failures = new List<FailureDetail>
                {
                    new($"Order {input.OrderId} does not exist", "Order")
                };
                return UseCaseResult<OrderDto>.NotFound(
                    "Order not found",
                    failures);
            }

            var orderDto = new OrderDto(
                order.Id,
                order.CustomerId,
                order.Status.ToString(),
                order.Currency,
                order.Items
                    .Select(item => new OrderItemDto(item.ProductId, item.UnitPrice, item.Quantity))
                    .ToList(),
                order.Total,
                order.CreatedAt);

            return UseCaseResult<OrderDto>.Success(orderDto, "Order retrieved successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error retrieving order {OrderId}", input.OrderId);
            return UseCaseResult<OrderDto>.Failure($"An error occurred while retrieving the order: {ex.Message}");
        }
    }
}

