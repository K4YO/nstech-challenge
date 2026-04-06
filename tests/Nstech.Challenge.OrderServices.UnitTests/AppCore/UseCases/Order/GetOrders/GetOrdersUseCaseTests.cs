using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Validators_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;
using Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.OrderAggregate.Builders_;
using Xunit;
using OrderEntity = Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate.Order;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.UseCases.Order.GetOrders;

public class GetOrdersUseCaseTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<GetOrdersDto> _validator;
    private readonly GetOrdersUseCase _sut;

    public GetOrdersUseCaseTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _validator = new GetOrdersDtoValidator();
        var logger = Substitute.For<ILogger<GetOrdersUseCase>>();
        _sut = new GetOrdersUseCase(_orderRepository, _validator, logger);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrdersUseCase")]
    public async Task GetOrdersUseCase_ExecuteAsync_WithNoFilters_ReturnsSuccess()
    {
        // Arrange
        var order1 = new OrderBuilder().AddItem(Guid.NewGuid(), 100m, 1).Build().Value!;
        var order2 = new OrderBuilder().AddItem(Guid.NewGuid(), 200m, 2).Build().Value!;

        _orderRepository.CountByCriteriaAsync(
            Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(2);
        _orderRepository.GetByCriteriaAsync(
            Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<OrderEntity> { order1, order2 });

        var dto = new GetOrdersDto(null, null, null, null);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Orders.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
        result.Data.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrdersUseCase")]
    public async Task GetOrdersUseCase_ExecuteAsync_WithCustomerIdFilter_PassesFilterToRepository()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _orderRepository.CountByCriteriaAsync(
            customerId, Arg.Any<OrderStatus?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _orderRepository.GetByCriteriaAsync(
            customerId, Arg.Any<OrderStatus?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<OrderEntity>());

        var dto = new GetOrdersDto(customerId, null, null, null);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Orders.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrdersUseCase")]
    public async Task GetOrdersUseCase_ExecuteAsync_WithStatusFilter_PassesFilterToRepository()
    {
        // Arrange
        _orderRepository.CountByCriteriaAsync(
            Arg.Any<Guid?>(), OrderStatus.Placed, Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _orderRepository.GetByCriteriaAsync(
            Arg.Any<Guid?>(), OrderStatus.Placed, Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<OrderEntity> { new OrderBuilder().AddItem(Guid.NewGuid(), 50m, 1).Build().Value! });

        var dto = new GetOrdersDto(null, "Placed", null, null);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Orders.Should().HaveCount(1);
        result.Data.Orders[0].Status.Should().Be("Placed");
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrdersUseCase")]
    public async Task GetOrdersUseCase_ExecuteAsync_WithInvalidStatus_ReturnsFailure()
    {
        // Arrange
        var dto = new GetOrdersDto(null, "InvalidStatus", null, null);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrdersUseCase")]
    public async Task GetOrdersUseCase_ExecuteAsync_WithPagination_PassesPaginationToRepository()
    {
        // Arrange
        _orderRepository.CountByCriteriaAsync(
            Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(25);
        _orderRepository.GetByCriteriaAsync(
            Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
            2, 5, Arg.Any<CancellationToken>())
            .Returns(new List<OrderEntity>());

        var dto = new GetOrdersDto(null, null, null, null, Page: 2, PageSize: 5);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Page.Should().Be(2);
        result.Data.PageSize.Should().Be(5);
        result.Data.TotalCount.Should().Be(25);
        result.Data.TotalPages.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrdersUseCase")]
    public async Task GetOrdersUseCase_ExecuteAsync_WithInvalidPage_ReturnsValidationFailure()
    {
        // Arrange
        var dto = new GetOrdersDto(null, null, null, null, Page: 0, PageSize: 10);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.Unprocessable);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrdersUseCase")]
    public async Task GetOrdersUseCase_ExecuteAsync_WithPageSizeExceedingMax_ReturnsValidationFailure()
    {
        // Arrange
        var dto = new GetOrdersDto(null, null, null, null, Page: 1, PageSize: 101);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(UseCaseResultType.Unprocessable);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrdersUseCase")]
    public async Task GetOrdersUseCase_ExecuteAsync_WithDateRange_PassesDatesToRepository()
    {
        // Arrange
        var fromDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2025, 1, 31, 23, 59, 59, DateTimeKind.Utc);

        _orderRepository.CountByCriteriaAsync(
            Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(), fromDate, toDate, Arg.Any<CancellationToken>())
            .Returns(0);
        _orderRepository.GetByCriteriaAsync(
            Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(), fromDate, toDate,
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<OrderEntity>());

        var dto = new GetOrdersDto(null, null, fromDate, toDate);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "GetOrdersUseCase")]
    public async Task GetOrdersUseCase_ExecuteAsync_WithEmptyResults_ReturnsEmptyList()
    {
        // Arrange
        _orderRepository.CountByCriteriaAsync(
            Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _orderRepository.GetByCriteriaAsync(
            Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<OrderEntity>());

        var dto = new GetOrdersDto(null, null, null, null);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Orders.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);
        result.Data.TotalPages.Should().Be(0);
    }
}
