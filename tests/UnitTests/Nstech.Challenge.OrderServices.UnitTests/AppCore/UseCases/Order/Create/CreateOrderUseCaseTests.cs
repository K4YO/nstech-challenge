using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Validators_;
using OrderEntity = Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate.Order;
using Xunit;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.UseCases.Order.Create;

public class CreateOrderUseCaseTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateOrderDto> _validator;
    private readonly CreateOrderUseCase _sut;

    public CreateOrderUseCaseTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _validator = new CreateOrderDtoValidator();
        _sut = new CreateOrderUseCase(_orderRepository, _productRepository, _validator);
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CreateOrderUseCase")]
    public async Task CreateOrderUseCase_ExecuteAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productResult = Product.Create(productId, "SKU001", 100m, 10);
        var product = productResult.Value!;

        var dto = new CreateOrderDto(customerId, "USD", new List<CreateOrderItemDto>
        {
            new(productId, 2)
        });

        _productRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([product]);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);
        await _orderRepository.Received(1).AddAsync(Arg.Any<OrderEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CreateOrderUseCase")]
    public async Task CreateOrderUseCase_ExecuteAsync_WithInvalidCurrency_ReturnsFail()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var dto = new CreateOrderDto(customerId, "", new List<CreateOrderItemDto>());

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CreateOrderUseCase")]
    public async Task CreateOrderUseCase_ExecuteAsync_WithoutItems_ReturnsFail()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var dto = new CreateOrderDto(customerId, "USD", new List<CreateOrderItemDto>());

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CreateOrderUseCase")]
    public async Task CreateOrderUseCase_ExecuteAsync_WithProductNotFound_ReturnsFail()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var dto = new CreateOrderDto(customerId, "USD", new List<CreateOrderItemDto>
        {
            new(productId, 2)
        });

        _productRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product>());

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    [Trait("Category", "UseCase")]
    [Trait("Sut", "CreateOrderUseCase")]
    public async Task CreateOrderUseCase_ExecuteAsync_WithInsufficientStock_ReturnsFail()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productResult = Product.Create(productId, "SKU001", 100m, 1);
        var product = productResult.Value!;

        var dto = new CreateOrderDto(customerId, "USD", new List<CreateOrderItemDto>
        {
            new(productId, 5)
        });

        _productRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([product]);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Insufficient stock");
    }
}




