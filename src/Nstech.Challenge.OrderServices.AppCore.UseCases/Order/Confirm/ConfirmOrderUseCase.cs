using FluentValidation;
using Microsoft.Extensions.Logging;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm;

public class ConfirmOrderUseCase : UseCase<ConfirmOrderDto, ConfirmOrderResultDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmOrderUseCase(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IValidator<ConfirmOrderDto> validator,
        ILogger<ConfirmOrderUseCase> logger) : base(logger, validator)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    protected override UseCaseResult<ConfirmOrderResultDto> HandleValidationFailure(Dictionary<string, string[]> errors)
    {
        var failures = errors.SelectMany(kvp => kvp.Value.Select(msg => new FailureDetail(msg, kvp.Key))).ToList();
        return UseCaseResult<ConfirmOrderResultDto>.Unprocessable("Validation failed", failures);
    }

    protected override async Task<UseCaseResult<ConfirmOrderResultDto>> ExecuteValidatedAsync(
        ConfirmOrderDto input,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(input.OrderId, cancellationToken);
            if (order == null)
            {
                Logger.LogWarning("Order {OrderId} not found for confirmation", input.OrderId);
                var failures = new List<FailureDetail>
                {
                    new($"Order {input.OrderId} does not exist", "Order")
                };
                return UseCaseResult<ConfirmOrderResultDto>.NotFound("Order not found", failures);
            }

            if (order.IsConfirmed())
            {
                Logger.LogInformation("Order {OrderId} is already confirmed (idempotent)", order.Id);
                var resultDto = new ConfirmOrderResultDto(order.Id, order.Status.ToString());
                return UseCaseResult<ConfirmOrderResultDto>.Success(resultDto, "Order is already confirmed");
            }

            var confirmResult = order.Confirm();
            if (!confirmResult.IsSuccess)
            {
                var failures = confirmResult.Failures
                    .Select(f => new FailureDetail(f.Description, f.Tag))
                    .ToList();
                return UseCaseResult<ConfirmOrderResultDto>.Failure("Cannot confirm order", failures);
            }

            var productIds = order.Items.Select(x => x.ProductId).Distinct().ToList();
            var products = (await _productRepository.GetByIdsAsync(productIds, cancellationToken))
                .ToDictionary(p => p.Id);

            foreach (var item in order.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    var failures = new List<FailureDetail>
                    {
                        new($"Product {item.ProductId} does not exist", "Products")
                    };
                    return UseCaseResult<ConfirmOrderResultDto>.Unprocessable(
                        $"Product {item.ProductId} not found",
                        failures);
                }

                var reserveResult = product.ReserveStock(item.Quantity);
                if (!reserveResult.IsSuccess)
                {
                    var failures = reserveResult.Failures
                        .Select(f => new FailureDetail(f.Description, f.Tag))
                        .ToList();
                    return UseCaseResult<ConfirmOrderResultDto>.Unprocessable("Cannot reserve stock", failures);
                }

                await _productRepository.UpdateAsync(product, cancellationToken);
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Order {OrderId} confirmed successfully", order.Id);

            var confirmedOrderDto = new ConfirmOrderResultDto(order.Id, order.Status.ToString());
            return UseCaseResult<ConfirmOrderResultDto>.Success(confirmedOrderDto, "Order confirmed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error confirming order {OrderId}", input.OrderId);
            return UseCaseResult<ConfirmOrderResultDto>.Failure($"An error occurred while confirming the order: {ex.Message}");
        }
    }
}

