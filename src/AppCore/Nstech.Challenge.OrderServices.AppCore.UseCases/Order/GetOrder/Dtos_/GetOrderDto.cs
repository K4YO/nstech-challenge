namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Dtos_;

public sealed record OrderItemDto(Guid ProductId, decimal UnitPrice, int Quantity)
{
    public decimal Total => UnitPrice * Quantity;
}

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    string Currency,
    List<OrderItemDto> Items,
    decimal Total,
    DateTime CreatedAt);

public sealed record GetOrderDto(Guid OrderId);
