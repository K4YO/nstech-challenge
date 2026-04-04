using FluentValidation;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders;

public class GetOrdersUseCase : UseCase<GetOrdersDto, UseCaseResult<GetOrdersResultDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<GetOrdersDto> _validator;

    public GetOrdersUseCase(IOrderRepository orderRepository, IValidator<GetOrdersDto> validator)
    {
        _orderRepository = orderRepository;
        _validator = validator;
    }

    public override async Task<UseCaseResult<GetOrdersResultDto>> ExecuteAsync(
        GetOrdersDto input,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return UseCaseResult<GetOrdersResultDto>.Failure("Validation failed", errors);
        }

        try
        {
            var status = string.IsNullOrEmpty(input.Status) ? (OrderStatus?)null : Enum.Parse<OrderStatus>(input.Status);

            var orders = await _orderRepository.GetByCriteriaAsync(
                customerId: input.CustomerId,
                status: status,
                fromDate: input.FromDate,
                toDate: input.ToDate,
                pageNumber: input.Page,
                pageSize: input.PageSize,
                cancellationToken: cancellationToken);

            var ordersList = orders.ToList();

            var result = new GetOrdersResultDto(
                ordersList
                    .Select(order => new OrderSummaryDto(
                        order.Id,
                        order.CustomerId,
                        order.Status.ToString(),
                        order.Currency,
                        order.Total,
                        order.CreatedAt))
                    .ToList(),
                ordersList.Count,
                input.Page,
                input.PageSize);

            return UseCaseResult<GetOrdersResultDto>.Success(result, "Orders retrieved successfully");
        }
        catch (Exception ex)
        {
            return UseCaseResult<GetOrdersResultDto>.Failure($"An error occurred while retrieving orders: {ex.Message}");
        }
    }
}
