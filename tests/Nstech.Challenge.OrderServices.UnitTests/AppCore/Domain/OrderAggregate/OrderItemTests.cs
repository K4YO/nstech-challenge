using FluentAssertions;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Xunit;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.OrderAggregate;

public class OrderItemTests
{
    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderItem")]
    public void OrderItem_Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var unitPrice = 50m;
        var quantity = 3;

        // Act
        var result = OrderItem.Create(productId, unitPrice, quantity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ProductId.Should().Be(productId);
        result.Value.UnitPrice.Should().Be(unitPrice);
        result.Value.Quantity.Should().Be(quantity);
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderItem")]
    public void OrderItem_Create_WithEmptyProductId_ReturnsFailure()
    {
        // Arrange
        var productId = Guid.Empty;
        var unitPrice = 50m;
        var quantity = 3;

        // Act
        var result = OrderItem.Create(productId, unitPrice, quantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "PRODUCT_ID_REQUIRED");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderItem")]
    public void OrderItem_Create_WithInvalidUnitPrice_ReturnsFailure(decimal unitPrice)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantity = 3;

        // Act
        var result = OrderItem.Create(productId, unitPrice, quantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "INVALID_UNIT_PRICE");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderItem")]
    public void OrderItem_Create_WithInvalidQuantity_ReturnsFailure(int quantity)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var unitPrice = 50m;

        // Act
        var result = OrderItem.Create(productId, unitPrice, quantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "INVALID_QUANTITY");
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderItem")]
    public void OrderItem_Create_WithMultipleInvalidFields_ReturnsAllFailures()
    {
        // Arrange
        var productId = Guid.Empty;
        var unitPrice = 0m;
        var quantity = 0;

        // Act
        var result = OrderItem.Create(productId, unitPrice, quantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().HaveCount(3);
        result.Failures.Should().Contain(f => f.Code == "PRODUCT_ID_REQUIRED");
        result.Failures.Should().Contain(f => f.Code == "INVALID_UNIT_PRICE");
        result.Failures.Should().Contain(f => f.Code == "INVALID_QUANTITY");
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderItem")]
    public void OrderItem_GetTotal_ReturnsCorrectValue()
    {
        // Arrange
        var result = OrderItem.Create(Guid.NewGuid(), 25.50m, 4);
        var item = result.Value!;

        // Act
        var total = item.GetTotal();

        // Assert
        total.Should().Be(102m);
    }
}
