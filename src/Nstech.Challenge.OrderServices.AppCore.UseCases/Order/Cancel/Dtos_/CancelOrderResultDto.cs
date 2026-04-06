using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Dtos_;

/// <summary>
/// Response DTO for CancelOrderUseCase.
/// Represents the result of a cancel order operation.
/// </summary>
public sealed record CancelOrderResultDto(
    Guid OrderId,
    string Status) : Dto;
