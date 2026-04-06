using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.OrderAggregate.Builders_;

public class OrderBuilder
{
    private Guid _customerId = Guid.NewGuid();
    private string _currency = "USD";
    private List<OrderItem> _items = [];

    public OrderBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public OrderBuilder AddItem(Guid productId, decimal unitPrice, int quantity)
    {
        var itemResult = OrderItem.Create(productId, unitPrice, quantity);
        if (itemResult.IsSuccess)
        {
            _items.Add(itemResult.Value!);
        }
        return this;
    }

    public OrderBuilder WithItems(List<OrderItem> items)
    {
        _items = items;
        return this;
    }

    public ValueResult<Order> Build()
    {
        return Order.Create(_customerId, _currency, _items);
    }
}


