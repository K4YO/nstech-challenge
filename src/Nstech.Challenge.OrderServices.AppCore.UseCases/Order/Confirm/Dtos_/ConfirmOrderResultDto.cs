using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Dtos_;

/// <summary>
/// Response DTO for ConfirmOrderUseCase.
/// Represents the result of a confirm order operation.
/// </summary>
public sealed record ConfirmOrderResultDto(
    Guid OrderId,
    string Status) : Dto;
