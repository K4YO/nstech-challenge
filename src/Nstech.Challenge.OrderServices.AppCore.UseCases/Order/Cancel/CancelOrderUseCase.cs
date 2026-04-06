using FluentValidation;
using Microsoft.Extensions.Logging;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel;

public class CancelOrderUseCase : UseCase<CancelOrderDto, CancelOrderResultDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderUseCase(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IValidator<CancelOrderDto> validator,
        ILogger<CancelOrderUseCase> logger) : base(logger, validator)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<UseCaseResult<CancelOrderResultDto>> ExecuteValidatedAsync(
        CancelOrderDto input,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(input.OrderId, cancellationToken);
            if (order == null)
            {
                Logger.LogWarning("Order {OrderId} not found for cancellation", input.OrderId);
                var failures = new List<FailureDetail>
                {
                    new($"Order {input.OrderId} does not exist", "Order")
                };
                return UseCaseResult<CancelOrderResultDto>.NotFound("Order not found", failures);
            }

            if (order.IsCanceled())
            {
                Logger.LogInformation("Order {OrderId} is already canceled (idempotent)", order.Id);
                var resultDto = new CancelOrderResultDto(order.Id, order.Status.ToString());
                return UseCaseResult<CancelOrderResultDto>.Success(resultDto, "Order is already canceled");
            }

            var wasConfirmed = order.IsConfirmed();

            // Usar ValueResult do domain
            var cancelResult = order.Cancel();
            if (!cancelResult.IsSuccess)
            {
                var failures = cancelResult.Failures
                    .Select(f => new FailureDetail(f.Description, f.Tag))
                    .ToList();
                return UseCaseResult<CancelOrderResultDto>.Unprocessable("Cannot cancel order", failures);
            }

            if (wasConfirmed)
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
                            var failures = releaseResult.Failures
                                .Select(f => new FailureDetail(f.Description, f.Tag))
                                .ToList();
                            return UseCaseResult<CancelOrderResultDto>.Unprocessable("Cannot release reserved stock", failures);
                        }

                        await _productRepository.UpdateAsync(product, cancellationToken);
                    }
                }
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Order {OrderId} canceled successfully (was confirmed: {WasConfirmed})", order.Id, wasConfirmed);

            var canceledOrderDto = new CancelOrderResultDto(order.Id, order.Status.ToString());
            return UseCaseResult<CancelOrderResultDto>.Success(canceledOrderDto, "Order canceled successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error canceling order {OrderId}", input.OrderId);
            return UseCaseResult<CancelOrderResultDto>.Failure($"An error occurred while canceling the order: {ex.Message}");
        }
    }
}



