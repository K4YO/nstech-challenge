using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Auth.GenereteToken.Dtos_;

/// <summary>
/// Response com token JWT gerado.
/// </summary>
public sealed record TokenDto(string Token, int ExpiresIn, string TokenType) : Dto;
