namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Dtos_;

public sealed record GetOrdersDto(Guid? CustomerId, string? Status, DateTime? FromDate, DateTime? ToDate, int Page = 1, int PageSize = 10);

public sealed record OrderSummaryDto(Guid Id, Guid CustomerId, string Status, string Currency, decimal Total, DateTime CreatedAt);

public sealed record GetOrdersResultDto(
    List<OrderSummaryDto> Orders,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
