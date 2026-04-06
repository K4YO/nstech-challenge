using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

namespace Nstech.Challenge.OrderServices.UnitTests.AppCore.Domain.ProductAggregate.Builders_;

public class ProductBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _sku = "SKU001";
    private decimal _unitPrice = 100m;
    private int _availableQuantity = 10;

    public ProductBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithSku(string sku)
    {
        _sku = sku;
        return this;
    }

    public ProductBuilder WithUnitPrice(decimal unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public ProductBuilder WithAvailableQuantity(int availableQuantity)
    {
        _availableQuantity = availableQuantity;
        return this;
    }

    public ValueResult<Product> Build()
    {
        return Product.Create(_id, _sku, _unitPrice, _availableQuantity);
    }
}
