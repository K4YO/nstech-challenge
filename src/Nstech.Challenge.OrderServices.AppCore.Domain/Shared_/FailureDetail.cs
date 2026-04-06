namespace Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

/// <summary>
/// Representa um detalhe de falha em uma operação com descrição obrigatória,
/// tag e código opcionais. Implementação do padrão Notification.
/// </summary>
public sealed record FailureDetail(string Description, string? Tag = null, string? Code = null);
