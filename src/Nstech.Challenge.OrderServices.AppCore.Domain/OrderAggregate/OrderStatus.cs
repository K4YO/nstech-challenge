using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;

public sealed class OrderStatus : ValueObject
{
    private readonly int _value;

    public int Value => _value;

    public static readonly OrderStatus Draft = new(0);
    public static readonly OrderStatus Placed = new(1);
    public static readonly OrderStatus Confirmed = new(2);
    public static readonly OrderStatus Canceled = new(3);

    private OrderStatus(int value)
    {
        _value = value;
    }

    public override bool Equals(ValueObject? other)
    {
        if (other is not OrderStatus orderStatus)
        {
            return false;
        }

        return _value == orderStatus._value;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString() => _value switch
    {
        0 => "Draft",
        1 => "Placed",
        2 => "Confirmed",
        3 => "Canceled",
        _ => "Unknown"
    };

    public static OrderStatus FromValue(int value) => value switch
    {
        0 => Draft,
        1 => Placed,
        2 => Confirmed,
        3 => Canceled,
        _ => throw new ArgumentException($"Invalid OrderStatus value: {value}", nameof(value))
    };

    /// <summary>
    /// Creates an OrderStatus from its string name representation.
    /// </summary>
    public static OrderStatus FromString(string name) => name switch
    {
        "Draft" => Draft,
        "Placed" => Placed,
        "Confirmed" => Confirmed,
        "Canceled" => Canceled,
        _ => throw new ArgumentException($"Invalid OrderStatus name: {name}", nameof(name))
    };
}
