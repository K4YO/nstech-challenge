using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;



public class Order : Entity
{
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string Currency { get; private set; }
    public decimal Total { get; private set; }


    private readonly List<OrderItem> _items = [];

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order()
    {
        Currency = string.Empty;
    }

    private Order(Guid id, Guid customerId, string currency, List<OrderItem> items)
        : base(id)
    {
        CustomerId = customerId;
        Currency = currency;
        Status = OrderStatus.Placed;
        CreatedAt = DateTime.UtcNow;
        _items = items;
        Total = _items.Sum(item => item.UnitPrice * item.Quantity);
    }

    public static ValueResult<Order> Create(Guid customerId, string currency, List<OrderItem> items)
    {
        var failures = new List<FailureDetail>();

        // Validação de moeda
        if (string.IsNullOrWhiteSpace(currency))
        {
            failures.Add(new FailureDetail("Currency cannot be null or empty", "Currency", "CURRENCY_REQUIRED"));
        }

        // Validação de itens
        if (items == null || items.Count == 0)
        {
            failures.Add(new FailureDetail("Order must have at least one item", "Items", "ITEMS_REQUIRED"));
        }
        else
        {
            // Validação de quantidade
            foreach (var item in items)
            {
                if (item.Quantity <= 0)
                {
                    failures.Add(new FailureDetail($"Item {item.ProductId} quantity must be greater than 0", "ItemQuantity", "INVALID_QUANTITY"));
                }
            }
        }

        if (failures.Any())
        {
            return ValueResult<Order>.Failure(failures);
        }

        var order = new Order(Guid.NewGuid(), customerId, currency, items);
        return ValueResult<Order>.Success(order);
    }

    public ValueResult<Order> Confirm()
    {
        if (Status != OrderStatus.Placed)
        {
            return ValueResult<Order>.Failure(
                $"Cannot confirm order in {Status} status. Only orders in Placed status can be confirmed.",
                "OrderStatus",
                "INVALID_STATUS_FOR_CONFIRM");
        }

        Status = OrderStatus.Confirmed;
        return ValueResult<Order>.Success(this);
    }

    public ValueResult<Order> Cancel()
    {
        if (Status != OrderStatus.Placed && Status != OrderStatus.Confirmed)
        {
            return ValueResult<Order>.Failure(
                $"Cannot cancel order in {Status} status. Only orders in Placed or Confirmed status can be canceled.",
                "OrderStatus",
                "INVALID_STATUS_FOR_CANCEL");
        }

        Status = OrderStatus.Canceled;
        return ValueResult<Order>.Success(this);
    }

    public bool IsConfirmed() => Status == OrderStatus.Confirmed;

    public bool IsCanceled() => Status == OrderStatus.Canceled;

    public bool IsPlaced() => Status == OrderStatus.Placed;
}

