using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Dtos_;

public sealed record ConfirmOrderDto(Guid OrderId) : Dto;

