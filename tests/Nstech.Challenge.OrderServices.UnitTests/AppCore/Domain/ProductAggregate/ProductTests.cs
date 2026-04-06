using FluentAssertions;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.ProductAggregate.Builders_;
using Xunit;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.ProductAggregate;

public class ProductTests
{
    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sku = "SKU001";
        var unitPrice = 99.99m;
        var availableQuantity = 50;

        // Act
        var result = Product.Create(id, sku, unitPrice, availableQuantity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(id);
        result.Value.Sku.Should().Be(sku);
        result.Value.UnitPrice.Should().Be(unitPrice);
        result.Value.AvailableQuantity.Should().Be(availableQuantity);
        result.Value.ReservedQuantity.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_Create_WithInvalidSku_ReturnsFailure(string sku)
    {
        // Arrange & Act
        var result = Product.Create(Guid.NewGuid(), sku, 10m, 5);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "SKU_REQUIRED");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_Create_WithInvalidUnitPrice_ReturnsFailure(decimal unitPrice)
    {
        // Arrange & Act
        var result = Product.Create(Guid.NewGuid(), "SKU001", unitPrice, 5);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "INVALID_UNIT_PRICE");
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_Create_WithNegativeAvailableQuantity_ReturnsFailure()
    {
        // Arrange & Act
        var result = Product.Create(Guid.NewGuid(), "SKU001", 10m, -1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "INVALID_QUANTITY");
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_Create_WithMultipleInvalidFields_ReturnsAllFailures()
    {
        // Arrange & Act
        var result = Product.Create(Guid.NewGuid(), "", 0m, -1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().HaveCount(3);
        result.Failures.Should().Contain(f => f.Code == "SKU_REQUIRED");
        result.Failures.Should().Contain(f => f.Code == "INVALID_UNIT_PRICE");
        result.Failures.Should().Contain(f => f.Code == "INVALID_QUANTITY");
    }

    [Theory]
    [InlineData(10, 5, true)]
    [InlineData(5, 5, true)]
    [InlineData(3, 5, false)]
    [InlineData(0, 1, false)]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_HasSufficientStock_ReturnsExpectedResult(int available, int required, bool expected)
    {
        // Arrange
        var product = new ProductBuilder()
            .WithAvailableQuantity(available)
            .Build().Value!;

        // Act
        var result = product.HasSufficientStock(required);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_ReserveStock_WithValidQuantity_UpdatesStockCorrectly()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithAvailableQuantity(10)
            .Build().Value!;

        // Act
        var result = product.ReserveStock(3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.AvailableQuantity.Should().Be(7);
        product.ReservedQuantity.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_ReserveStock_WithInsufficientStock_ReturnsFailure()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithAvailableQuantity(2)
            .Build().Value!;

        // Act
        var result = product.ReserveStock(5);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "INSUFFICIENT_STOCK");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_ReserveStock_WithInvalidQuantity_ReturnsFailure(int quantity)
    {
        // Arrange
        var product = new ProductBuilder()
            .WithAvailableQuantity(10)
            .Build().Value!;

        // Act
        var result = product.ReserveStock(quantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "INVALID_QUANTITY");
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_ReleaseReservedStock_WithValidQuantity_UpdatesStockCorrectly()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithAvailableQuantity(10)
            .Build().Value!;
        product.ReserveStock(5);

        // Act
        var result = product.ReleaseReservedStock(3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.AvailableQuantity.Should().Be(8);
        product.ReservedQuantity.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_ReleaseReservedStock_WithMoreThanReserved_ReturnsFailure()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithAvailableQuantity(10)
            .Build().Value!;
        product.ReserveStock(2);

        // Act
        var result = product.ReleaseReservedStock(5);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "INVALID_RELEASE");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Product")]
    public void Product_ReleaseReservedStock_WithInvalidQuantity_ReturnsFailure(int quantity)
    {
        // Arrange
        var product = new ProductBuilder()
            .WithAvailableQuantity(10)
            .Build().Value!;
        product.ReserveStock(5);

        // Act
        var result = product.ReleaseReservedStock(quantity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "INVALID_QUANTITY");
    }
}
