using FluentValidation;
using Microsoft.Extensions.Logging;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders;

public class GetOrdersUseCase : UseCase<GetOrdersDto, GetOrdersResultDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersUseCase(
        IOrderRepository orderRepository,
        IValidator<GetOrdersDto> validator,
        ILogger<GetOrdersUseCase> logger) : base(logger, validator)
    {
        _orderRepository = orderRepository;
    }

    protected override UseCaseResult<GetOrdersResultDto> HandleValidationFailure(Dictionary<string, string[]> errors)
    {
        var failures = errors.SelectMany(kvp => kvp.Value.Select(msg => new FailureDetail(msg, kvp.Key))).ToList();
        return UseCaseResult<GetOrdersResultDto>.Unprocessable("Validation failed", failures);
    }

    protected override async Task<UseCaseResult<GetOrdersResultDto>> ExecuteValidatedAsync(
        GetOrdersDto input,
        CancellationToken cancellationToken)
    {
        try
        {
            OrderStatus? status = null;
            if (!string.IsNullOrEmpty(input.Status))
            {
                status = input.Status switch
                {
                    "Draft" => OrderStatus.Draft,
                    "Placed" => OrderStatus.Placed,
                    "Confirmed" => OrderStatus.Confirmed,
                    "Canceled" => OrderStatus.Canceled,
                    _ => null
                };

                if (status == null)
                {
                    var failures = new List<FailureDetail>
                    {
                        new($"Invalid status: {input.Status}", "Status", "INVALID_STATUS")
                    };
                    return UseCaseResult<GetOrdersResultDto>.Failure(
                        $"Invalid status: {input.Status}",
                        failures);
                }
            }

            var totalCount = await _orderRepository.CountByCriteriaAsync(
                customerId: input.CustomerId,
                status: status,
                fromDate: input.FromDate,
                toDate: input.ToDate,
                cancellationToken: cancellationToken);

            var orders = await _orderRepository.GetByCriteriaAsync(
                customerId: input.CustomerId,
                status: status,
                fromDate: input.FromDate,
                toDate: input.ToDate,
                pageNumber: input.Page,
                pageSize: input.PageSize,
                cancellationToken: cancellationToken);

            var result = new GetOrdersResultDto(
                orders
                    .Select(order => new OrderSummaryDto(
                        order.Id,
                        order.CustomerId,
                        order.Status.ToString(),
                        order.Currency,
                        order.Total,
                        order.CreatedAt))
                    .ToList(),
                totalCount,
                input.Page,
                input.PageSize);

            return UseCaseResult<GetOrdersResultDto>.Success(result, "Orders retrieved successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error retrieving orders");
            return UseCaseResult<GetOrdersResultDto>.Failure($"An error occurred while retrieving orders: {ex.Message}");
        }
    }
}

