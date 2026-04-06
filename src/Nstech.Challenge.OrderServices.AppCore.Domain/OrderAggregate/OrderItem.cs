using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;

public class OrderItem : Entity
{
    public Guid ProductId { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem()
    {
    }

    private OrderItem(Guid productId, decimal unitPrice, int quantity) : base()
    {
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public static ValueResult<OrderItem> Create(Guid productId, decimal unitPrice, int quantity)
    {
        var failures = new List<FailureDetail>();

        if (productId == Guid.Empty)
        {
            failures.Add(new FailureDetail("ProductId cannot be empty", "ProductId", "PRODUCT_ID_REQUIRED"));
        }

        if (unitPrice <= 0)
        {
            failures.Add(new FailureDetail("UnitPrice must be greater than 0", "UnitPrice", "INVALID_UNIT_PRICE"));
        }

        if (quantity <= 0)
        {
            failures.Add(new FailureDetail("Quantity must be greater than 0", "Quantity", "INVALID_QUANTITY"));
        }

        if (failures.Any())
        {
            return ValueResult<OrderItem>.Failure(failures);
        }

        return ValueResult<OrderItem>.Success(new OrderItem(productId, unitPrice, quantity));
    }

    public decimal GetTotal() => UnitPrice * Quantity;
}


