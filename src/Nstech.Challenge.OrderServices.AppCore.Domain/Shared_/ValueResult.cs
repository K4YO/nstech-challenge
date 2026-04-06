namespace Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

/// <summary>
/// Resultado genérico de uma operação que retorna um valor em caso de sucesso.
/// Implementa o padrão Notification para tratamento de erros sem exceções.
/// </summary>
public sealed record ValueResult<TValue>
{
    public bool IsSuccess { get; }
    public TValue? Value { get; }
    public IReadOnlyList<FailureDetail> Failures { get; }

    private ValueResult(bool isSuccess, TValue? value, List<FailureDetail> failures)
    {
        IsSuccess = isSuccess;
        Value = value;
        Failures = failures.AsReadOnly();
    }

    public static ValueResult<TValue> Success(TValue value) => new(true, value, []);
    
    public static ValueResult<TValue> Failure(string description, string? tag = null, string? code = null) =>
        new(false, default, [new FailureDetail(description, tag, code)]);

    public static ValueResult<TValue> Failure(List<FailureDetail> failures) =>
        new(false, default, failures);
}
