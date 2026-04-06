using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Auth.GenereteToken.Dtos_;

/// <summary>
/// Request para geração de token JWT.
/// </summary>
public sealed record CreateTokenDto(string Username, string Password) : Dto;
