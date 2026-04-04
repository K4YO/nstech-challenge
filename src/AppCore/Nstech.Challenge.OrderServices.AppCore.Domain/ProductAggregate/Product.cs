using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;

public class Product : Entity
{
    public string Sku { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int AvailableQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }

    private Product()
    {
        Sku = string.Empty;
    }

    private Product(Guid id, string sku, decimal unitPrice, int availableQuantity)
        : base(id)
    {
        Sku = sku;
        UnitPrice = unitPrice;
        AvailableQuantity = availableQuantity;
        ReservedQuantity = 0;
    }

    public static ValueResult<Product> Create(Guid id, string sku, decimal unitPrice, int availableQuantity)
    {
        var failures = new List<FailureDetail>();

        if (string.IsNullOrWhiteSpace(sku))
        {
            failures.Add(new FailureDetail("SKU cannot be null or empty", "SKU", "SKU_REQUIRED"));
        }

        if (unitPrice <= 0)
        {
            failures.Add(new FailureDetail("UnitPrice must be greater than 0", "UnitPrice", "INVALID_UNIT_PRICE"));
        }

        if (availableQuantity < 0)
        {
            failures.Add(new FailureDetail("AvailableQuantity cannot be negative", "AvailableQuantity", "INVALID_QUANTITY"));
        }

        if (failures.Any())
        {
            return ValueResult<Product>.CreateFailure(failures);
        }

        return ValueResult<Product>.CreateSuccess(new Product(id, sku, unitPrice, availableQuantity));
    }

    public bool HasSufficientStock(int requiredQuantity)
    {
        return AvailableQuantity >= requiredQuantity;
    }

    public ValueResult ReserveStock(int quantity)
    {
        if (quantity <= 0)
        {
            return ValueResult.CreateFailure("Quantity must be greater than 0", "Quantity", "INVALID_QUANTITY");
        }

        if (!HasSufficientStock(quantity))
        {
            return ValueResult.CreateFailure($"Insufficient stock. Available: {AvailableQuantity}, Requested: {quantity}", "Stock", "INSUFFICIENT_STOCK");
        }

        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;

        return ValueResult.CreateSuccess();
    }

    public ValueResult ReleaseReservedStock(int quantity)
    {
        if (quantity <= 0)
        {
            return ValueResult.CreateFailure("Quantity must be greater than 0", "Quantity", "INVALID_QUANTITY");
        }

        if (ReservedQuantity < quantity)
        {
            return ValueResult.CreateFailure($"Cannot release more than reserved. Reserved: {ReservedQuantity}, Release: {quantity}", "Stock", "INVALID_RELEASE");
        }

        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;

        return ValueResult.CreateSuccess();
    }
}

