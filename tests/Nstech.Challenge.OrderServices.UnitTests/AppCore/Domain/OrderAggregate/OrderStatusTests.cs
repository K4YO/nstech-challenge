using FluentAssertions;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Xunit;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.OrderAggregate;

public class OrderStatusTests
{
    [Theory]
    [InlineData(0, "Draft")]
    [InlineData(1, "Placed")]
    [InlineData(2, "Confirmed")]
    [InlineData(3, "Canceled")]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderStatus")]
    public void OrderStatus_ToString_ReturnsExpectedName(int value, string expectedName)
    {
        // Arrange
        var status = OrderStatus.FromValue(value);

        // Act
        var result = status.ToString();

        // Assert
        result.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderStatus")]
    public void OrderStatus_FromValue_WithValidValue_ReturnsCorrectStatus(int value)
    {
        // Act
        var status = OrderStatus.FromValue(value);

        // Assert
        status.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(99)]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderStatus")]
    public void OrderStatus_FromValue_WithInvalidValue_ThrowsArgumentException(int value)
    {
        // Act
        var act = () => OrderStatus.FromValue(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Placed")]
    [InlineData("Confirmed")]
    [InlineData("Canceled")]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderStatus")]
    public void OrderStatus_FromString_WithValidName_ReturnsCorrectStatus(string name)
    {
        // Act
        var status = OrderStatus.FromString(name);

        // Assert
        status.ToString().Should().Be(name);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("")]
    [InlineData("placed")]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderStatus")]
    public void OrderStatus_FromString_WithInvalidName_ThrowsArgumentException(string name)
    {
        // Act
        var act = () => OrderStatus.FromString(name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderStatus")]
    public void OrderStatus_Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var status1 = OrderStatus.Placed;
        var status2 = OrderStatus.Placed;

        // Act & Assert
        status1.Should().Be(status2);
        (status1 == status2).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderStatus")]
    public void OrderStatus_Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var status1 = OrderStatus.Placed;
        var status2 = OrderStatus.Confirmed;

        // Act & Assert
        status1.Should().NotBe(status2);
        (status1 != status2).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderStatus")]
    public void OrderStatus_Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var status = OrderStatus.Placed;

        // Act & Assert
        status.Equals(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    [Trait("Sut", "OrderStatus")]
    public void OrderStatus_GetHashCode_SameValueHasSameHash()
    {
        // Arrange
        var status1 = OrderStatus.Confirmed;
        var status2 = OrderStatus.FromValue(2);

        // Act & Assert
        status1.GetHashCode().Should().Be(status2.GetHashCode());
    }
}
