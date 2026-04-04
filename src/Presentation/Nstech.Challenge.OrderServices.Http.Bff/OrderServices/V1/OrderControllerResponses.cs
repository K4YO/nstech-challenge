namespace Nstech.Challenge.OrderServices.Http.Bff.OrderServices.V1;

/// <summary>
/// Response quando um pedido é criado com sucesso.
/// </summary>
public sealed record OrderCreatedResponse(Guid OrderId);

/// <summary>
/// Response genérico de sucesso com mensagem opcional.
/// </summary>
public sealed record SuccessResponse(string? Message = null);

/// <summary>
/// Response genérico de erro com mensagem e detalhes de validação.
/// </summary>
public sealed record ErrorResponse(string? Message = null, Dictionary<string, string[]>? Errors = null);
