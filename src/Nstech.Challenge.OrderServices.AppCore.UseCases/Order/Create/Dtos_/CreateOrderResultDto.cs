using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Dtos_;

/// <summary>
/// Response DTO for CreateOrderUseCase.
/// Represents the result of a create order operation.
/// </summary>
public sealed record CreateOrderResultDto(
    Guid OrderId,
    string Status) : Dto;
