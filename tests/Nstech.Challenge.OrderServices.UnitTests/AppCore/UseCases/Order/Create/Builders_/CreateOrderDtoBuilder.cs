using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Dtos_;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.UseCases.Order.Create.Builders_;

public class CreateOrderDtoBuilder
{
    private Guid _customerId = Guid.NewGuid();
    private string _currency = "USD";
    private List<CreateOrderItemDto> _items = [];

    public CreateOrderDtoBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public CreateOrderDtoBuilder WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public CreateOrderDtoBuilder AddItem(Guid productId, int quantity)
    {
        _items.Add(new CreateOrderItemDto(productId, quantity));
        return this;
    }

    public CreateOrderDtoBuilder WithItems(List<CreateOrderItemDto> items)
    {
        _items = items;
        return this;
    }

    public CreateOrderDto Build()
    {
        return new CreateOrderDto(_customerId, _currency, _items);
    }
}

