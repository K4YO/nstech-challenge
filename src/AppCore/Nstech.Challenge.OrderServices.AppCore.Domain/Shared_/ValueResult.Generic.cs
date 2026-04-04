namespace Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

/// <summary>
/// Resultado genérico de uma operação que retorna um valor em caso de sucesso.
/// Implementa o padrão Notification para tratamento de erros sem exceções.
/// </summary>
public abstract class ValueResult<TValue>
{
    public bool IsSuccess { get; }
    public TValue? Value { get; }
    public IReadOnlyList<FailureDetail> Failures { get; }

    protected ValueResult(bool isSuccess, TValue? value, List<FailureDetail> failures)
    {
        IsSuccess = isSuccess;
        Value = value;
        Failures = failures.AsReadOnly();
    }

    public static Success CreateSuccess(TValue value) => new(value);
    
    public static Failure CreateFailure(string description, string? tag = null, string? code = null) =>
        new([new FailureDetail(description, tag, code)]);

    public static Failure CreateFailure(List<FailureDetail> failures) =>
        new(failures);

    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<IReadOnlyList<FailureDetail>, TResult> onFailure) =>
        IsSuccess
            ? onSuccess(Value!)
            : onFailure(Failures);

    public async Task<TResult> MatchAsync<TResult>(
        Func<TValue, Task<TResult>> onSuccess,
        Func<IReadOnlyList<FailureDetail>, Task<TResult>> onFailure) =>
        IsSuccess
            ? await onSuccess(Value!)
            : await onFailure(Failures);

    public sealed class Success : ValueResult<TValue>
    {
        public Success(TValue value) : base(true, value, []) { }
    }

    public sealed class Failure : ValueResult<TValue>
    {
        public Failure(List<FailureDetail> failures) : base(false, default, failures) { }
    }
}
