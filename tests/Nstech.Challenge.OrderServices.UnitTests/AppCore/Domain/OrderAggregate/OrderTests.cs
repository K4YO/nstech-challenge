using FluentAssertions;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Xunit;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.OrderAggregate;

public class OrderTests
{
    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Order")]
    public void Order_CreateOrder_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var currency = "USD";
        var productId = Guid.NewGuid();
        var itemResult = OrderItem.Create(productId, 100m, 2);
        var items = new List<OrderItem> { itemResult.Value! };

        // Act
        var result = Order.Create(customerId, currency, items);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CustomerId.Should().Be(customerId);
        result.Value.Currency.Should().Be(currency);
        result.Value.Status.Should().Be(OrderStatus.Placed);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Total.Should().Be(200m);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Order")]
    public void Order_CreateOrder_WithoutItems_ReturnsFail()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var currency = "USD";
        var items = new List<OrderItem>();

        // Act
        var result = Order.Create(customerId, currency, items);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().NotBeEmpty();
        result.Failures.Should().Contain(f => f.Code == "ITEMS_REQUIRED");
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Order")]
    public void Order_CreateOrder_WithInvalidCurrency_ReturnsFail()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var currency = "";
        var productId = Guid.NewGuid();
        var itemResult = OrderItem.Create(productId, 100m, 1);
        var items = new List<OrderItem> { itemResult.Value! };

        // Act
        var result = Order.Create(customerId, currency, items);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().NotBeEmpty();
        result.Failures.Should().Contain(f => f.Code == "CURRENCY_REQUIRED");
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Order")]
    public void Order_ConfirmOrder_WhenPlaced_ChangesStatusToConfirmed()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var currency = "USD";
        var productId = Guid.NewGuid();
        var itemResult = OrderItem.Create(productId, 100m, 1);
        var items = new List<OrderItem> { itemResult.Value! };
        var orderResult = Order.Create(customerId, currency, items);
        var order = orderResult.Value!;

        // Act
        var confirmResult = order.Confirm();

        // Assert
        confirmResult.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.IsConfirmed().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Order")]
    public void Order_ConfirmOrder_WhenAlreadyConfirmed_ReturnsFail()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var currency = "USD";
        var productId = Guid.NewGuid();
        var itemResult = OrderItem.Create(productId, 100m, 1);
        var items = new List<OrderItem> { itemResult.Value! };
        var orderResult = Order.Create(customerId, currency, items);
        var order = orderResult.Value!;
        order.Confirm();

        // Act
        var confirmResult = order.Confirm();

        // Assert
        confirmResult.IsSuccess.Should().BeFalse();
        confirmResult.Failures.Should().NotBeEmpty();
        confirmResult.Failures.Should().Contain(f => f.Code == "INVALID_STATUS_FOR_CONFIRM");
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Order")]
    public void Order_CancelOrder_WhenPlaced_ChangesStatusToCanceled()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var currency = "USD";
        var productId = Guid.NewGuid();
        var itemResult = OrderItem.Create(productId, 100m, 1);
        var items = new List<OrderItem> { itemResult.Value! };
        var orderResult = Order.Create(customerId, currency, items);
        var order = orderResult.Value!;

        // Act
        var cancelResult = order.Cancel();

        // Assert
        cancelResult.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
        order.IsCanceled().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Order")]
    public void Order_CancelOrder_WhenConfirmed_ChangesStatusToCanceled()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var currency = "USD";
        var productId = Guid.NewGuid();
        var itemResult = OrderItem.Create(productId, 100m, 1);
        var items = new List<OrderItem> { itemResult.Value! };
        var orderResult = Order.Create(customerId, currency, items);
        var order = orderResult.Value!;
        order.Confirm();

        // Act
        var cancelResult = order.Cancel();

        // Assert
        cancelResult.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
        order.IsCanceled().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Order")]
    public void Order_CancelOrder_WhenAlreadyCanceled_ReturnsFail()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var currency = "USD";
        var productId = Guid.NewGuid();
        var itemResult = OrderItem.Create(productId, 100m, 1);
        var items = new List<OrderItem> { itemResult.Value! };
        var orderResult = Order.Create(customerId, currency, items);
        var order = orderResult.Value!;
        order.Cancel();

        // Act
        var cancelResult = order.Cancel();

        // Assert
        cancelResult.IsSuccess.Should().BeFalse();
        cancelResult.Failures.Should().NotBeEmpty();
        cancelResult.Failures.Should().Contain(f => f.Code == "INVALID_STATUS_FOR_CANCEL");
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "Order")]
    public void Order_CalculatesTotalCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var currency = "USD";
        var item1Result = OrderItem.Create(Guid.NewGuid(), 100m, 2);
        var item2Result = OrderItem.Create(Guid.NewGuid(), 50m, 3);
        var items = new List<OrderItem> { item1Result.Value!, item2Result.Value! };

        // Act
        var orderResult = Order.Create(customerId, currency, items);

        // Assert
        orderResult.Value!.Total.Should().Be(350m); // (100 * 2) + (50 * 3)
    }
}


