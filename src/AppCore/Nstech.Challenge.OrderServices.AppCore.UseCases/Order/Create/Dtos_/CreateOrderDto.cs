namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Dtos_;

public sealed record CreateOrderItemDto(Guid ProductId, int Quantity);

public sealed record CreateOrderDto(Guid CustomerId, string Currency, List<CreateOrderItemDto> Items);
