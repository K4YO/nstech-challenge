using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Validators_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;
using Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.OrderAggregate.Builders_;
using Xunit;
using OrderEntity = Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate.Order;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.UseCases.Order.Confirm;

public class ConfirmOrderUseCaseTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ConfirmOrderDto> _validator;
    private readonly ConfirmOrderUseCase _sut;

    public ConfirmOrderUseCaseTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _validator = new ConfirmOrderDtoValidator();
        var logger = Substitute.For<ILogger<ConfirmOrderUseCase>>();
        _sut = new ConfirmOrderUseCase(_orderRepository, _productRepository, _unitOfWork, _validator, logger);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "ConfirmOrderUseCase")]
    public async Task ConfirmOrderUseCase_ExecuteAsync_WhenOrderIsPlaced_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = new OrderBuilder()
            .AddItem(productId, 100m, 2)
            .Build().Value!;

        var product = Product.Create(productId, "SKU001", 100m, 10).Value!;

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        _productRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product> { product });

        var dto = new ConfirmOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be("Confirmed");
        await _orderRepository.Received(1).UpdateAsync(Arg.Any<OrderEntity>(), Arg.Any<CancellationToken>());
        await _productRepository.Received(1).UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "ConfirmOrderUseCase")]
    public async Task ConfirmOrderUseCase_ExecuteAsync_WhenOrderNotFound_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((OrderEntity?)null);

        var dto = new ConfirmOrderDto(orderId);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.NotFound);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "ConfirmOrderUseCase")]
    public async Task ConfirmOrderUseCase_ExecuteAsync_WhenAlreadyConfirmed_ReturnsSuccessIdempotent()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = new OrderBuilder()
            .AddItem(productId, 100m, 2)
            .Build().Value!;
        order.Confirm();

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var dto = new ConfirmOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be("Confirmed");
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<OrderEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "ConfirmOrderUseCase")]
    public async Task ConfirmOrderUseCase_ExecuteAsync_WhenOrderCanceled_ReturnsFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = new OrderBuilder()
            .AddItem(productId, 100m, 2)
            .Build().Value!;
        order.Cancel();

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var dto = new ConfirmOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.Failure);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "ConfirmOrderUseCase")]
    public async Task ConfirmOrderUseCase_ExecuteAsync_WhenProductNotFound_ReturnsUnprocessable()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = new OrderBuilder()
            .AddItem(productId, 100m, 2)
            .Build().Value!;

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        _productRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product>());

        var dto = new ConfirmOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.Unprocessable);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "ConfirmOrderUseCase")]
    public async Task ConfirmOrderUseCase_ExecuteAsync_WhenInsufficientStock_ReturnsUnprocessable()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = new OrderBuilder()
            .AddItem(productId, 100m, 10)
            .Build().Value!;

        var product = Product.Create(productId, "SKU001", 100m, 2).Value!;

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        _productRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product> { product });

        var dto = new ConfirmOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.Unprocessable);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "ConfirmOrderUseCase")]
    public async Task ConfirmOrderUseCase_ExecuteAsync_WithEmptyOrderId_ReturnsValidationFailure()
    {
        // Arrange
        var dto = new ConfirmOrderDto(Guid.Empty);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.Unprocessable);
    }
}
