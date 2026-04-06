using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Validators_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;
using Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.OrderAggregate.Builders_;
using Xunit;
using OrderEntity = Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate.Order;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.UseCases.Order.Cancel;

public class CancelOrderUseCaseTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CancelOrderDto> _validator;
    private readonly CancelOrderUseCase _sut;

    public CancelOrderUseCaseTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _validator = new CancelOrderDtoValidator();
        var logger = Substitute.For<ILogger<CancelOrderUseCase>>();
        _sut = new CancelOrderUseCase(_orderRepository, _productRepository, _unitOfWork, _validator, logger);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CancelOrderUseCase")]
    public async Task CancelOrderUseCase_ExecuteAsync_WhenOrderIsPlaced_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = new OrderBuilder()
            .AddItem(productId, 100m, 2)
            .Build().Value!;

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var dto = new CancelOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be("Canceled");
        await _orderRepository.Received(1).UpdateAsync(Arg.Any<OrderEntity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CancelOrderUseCase")]
    public async Task CancelOrderUseCase_ExecuteAsync_WhenOrderIsConfirmed_ReleasesStockAndReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = new OrderBuilder()
            .AddItem(productId, 100m, 2)
            .Build().Value!;
        order.Confirm();

        var product = Product.Create(productId, "SKU001", 100m, 8).Value!;
        product.ReserveStock(2);

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        _productRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product> { product });

        var dto = new CancelOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be("Canceled");
        await _productRepository.Received(1).UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CancelOrderUseCase")]
    public async Task CancelOrderUseCase_ExecuteAsync_WhenOrderNotFound_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((OrderEntity?)null);

        var dto = new CancelOrderDto(orderId);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.NotFound);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CancelOrderUseCase")]
    public async Task CancelOrderUseCase_ExecuteAsync_WhenAlreadyCanceled_ReturnsSuccessIdempotent()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = new OrderBuilder()
            .AddItem(productId, 100m, 2)
            .Build().Value!;
        order.Cancel();

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var dto = new CancelOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be("Canceled");
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<OrderEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CancelOrderUseCase")]
    public async Task CancelOrderUseCase_ExecuteAsync_WhenPlacedOrder_DoesNotReleaseStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = new OrderBuilder()
            .AddItem(productId, 100m, 2)
            .Build().Value!;

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var dto = new CancelOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _productRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
        await _productRepository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CancelOrderUseCase")]
    public async Task CancelOrderUseCase_ExecuteAsync_WithEmptyOrderId_ReturnsValidationFailure()
    {
        // Arrange
        var dto = new CancelOrderDto(Guid.Empty);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.Unprocessable);
    }
}
