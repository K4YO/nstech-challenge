using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Validators_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;
using Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.OrderAggregate.Builders_;
using Xunit;
using OrderEntity = Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate.Order;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.UseCases.Order.GetOrder;

public class GetOrderUseCaseTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<GetOrderDto> _validator;
    private readonly GetOrderUseCase _sut;

    public GetOrderUseCaseTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _validator = new GetOrderDtoValidator();
        var logger = Substitute.For<ILogger<GetOrderUseCase>>();
        _sut = new GetOrderUseCase(_orderRepository, _validator, logger);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrderUseCase")]
    public async Task GetOrderUseCase_ExecuteAsync_WhenOrderExists_ReturnsSuccess()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = new OrderBuilder()
            .WithCustomerId(customerId)
            .WithCurrency("BRL")
            .AddItem(productId, 50m, 3)
            .Build().Value!;

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var dto = new GetOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(order.Id);
        result.Data.CustomerId.Should().Be(customerId);
        result.Data.Currency.Should().Be("BRL");
        result.Data.Status.Should().Be("Placed");
        result.Data.Total.Should().Be(150m);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items[0].ProductId.Should().Be(productId);
        result.Data.Items[0].UnitPrice.Should().Be(50m);
        result.Data.Items[0].Quantity.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrderUseCase")]
    public async Task GetOrderUseCase_ExecuteAsync_WhenOrderNotFound_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((OrderEntity?)null);

        var dto = new GetOrderDto(orderId);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.NotFound);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrderUseCase")]
    public async Task GetOrderUseCase_ExecuteAsync_WithEmptyOrderId_ReturnsValidationFailure()
    {
        // Arrange
        var dto = new GetOrderDto(Guid.Empty);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.Unprocessable);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrderUseCase")]
    public async Task GetOrderUseCase_ExecuteAsync_WithMultipleItems_ReturnsMappedItems()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var order = new OrderBuilder()
            .AddItem(productId1, 100m, 2)
            .AddItem(productId2, 50m, 5)
            .Build().Value!;

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var dto = new GetOrderDto(order.Id);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Total.Should().Be(450m);
    }
}
