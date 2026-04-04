namespace Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

/// <summary>
/// Resultado de uma operação que pode ter sucesso ou falha.
/// Implementa o padrão Notification para tratamento de erros sem exceções.
/// Utilizado para operações que não retornam valor.
/// </summary>
public abstract class ValueResult
{
    public bool IsSuccess { get; }
    public IReadOnlyList<FailureDetail> Failures { get; }

    protected ValueResult(bool isSuccess, List<FailureDetail> failures)
    {
        IsSuccess = isSuccess;
        Failures = failures.AsReadOnly();
    }

    public static Success CreateSuccess() => new();
    
    public static Failure CreateFailure(string description, string? tag = null, string? code = null) =>
        new([new FailureDetail(description, tag, code)]);

    public static Failure CreateFailure(List<FailureDetail> failures) =>
        new(failures);

    public sealed class Success : ValueResult
    {
        public Success() : base(true, []) { }
    }

    public sealed class Failure : ValueResult
    {
        public Failure(List<FailureDetail> failures) : base(false, failures) { }
    }
}


