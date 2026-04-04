namespace Nstech.Challenge.OrderServices.Http.Bff.Auth.V1;

/// <summary>
/// Request para geração de token JWT.
/// </summary>
public sealed record TokenRequest(string Username, string Password);

/// <summary>
/// Response com token JWT gerado.
/// </summary>
public sealed record TokenResponse(string Token, int ExpiresIn, string TokenType);

/// <summary>
/// Response genérico de erro com mensagem e detalhes de validação.
/// </summary>
public sealed record AuthErrorResponse(string? Message = null, Dictionary<string, string[]>? Errors = null);
