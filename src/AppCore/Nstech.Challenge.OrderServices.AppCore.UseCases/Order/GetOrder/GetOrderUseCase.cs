using FluentValidation;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder;

public class GetOrderUseCase : UseCase<GetOrderDto, UseCaseResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<GetOrderDto> _validator;

    public GetOrderUseCase(IOrderRepository orderRepository, IValidator<GetOrderDto> validator)
    {
        _orderRepository = orderRepository;
        _validator = validator;
    }

    public override async Task<UseCaseResult<OrderDto>> ExecuteAsync(
        GetOrderDto input,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return UseCaseResult<OrderDto>.Failure("Validation failed", errors);
        }

        try
        {
            var order = await _orderRepository.GetByIdAsync(input.OrderId, cancellationToken);
            if (order == null)
            {
                return UseCaseResult<OrderDto>.Failure(
                    "Order not found",
                    new Dictionary<string, string[]>
                    {
                        ["Order"] = new[] { $"Order {input.OrderId} does not exist" }
                    });
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
            return UseCaseResult<OrderDto>.Failure($"An error occurred while retrieving the order: {ex.Message}");
        }
    }
}
