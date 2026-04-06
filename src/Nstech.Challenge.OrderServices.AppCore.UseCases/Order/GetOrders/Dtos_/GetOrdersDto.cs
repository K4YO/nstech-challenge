using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Dtos_;

public sealed record GetOrdersDto(Guid? CustomerId, string? Status, DateTime? FromDate, DateTime? ToDate, int Page = 1, int PageSize = 10) : Dto;

public sealed record OrderSummaryDto(Guid Id, Guid CustomerId, string Status, string Currency, decimal Total, DateTime CreatedAt) : Dto;

public sealed record GetOrdersResultDto(
    List<OrderSummaryDto> Orders,
    int TotalCount,
    int Page,
    int PageSize) : Dto
{
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

