using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

public sealed record UseCaseResult<TData> : Dto where TData : Dto
{
    

    public bool IsSuccess { get; private set; }
    public UseCaseResultType Type { get; private set; }
    public TData? Data { get; private set; }
    public string? Message { get; private set; }
    public IReadOnlyList<FailureDetail> Failures { get; private set; }

    private UseCaseResult(bool isSuccess, UseCaseResultType type, TData? data, string? message, IReadOnlyList<FailureDetail> failures)
    {
        IsSuccess = isSuccess;
        Type = type;
        Data = data;
        Message = message;

        var failureList = failures is not null ? failures.ToList() : new List<FailureDetail>();
        if (!string.IsNullOrWhiteSpace(message))
        {
            failureList.Add(new FailureDetail(message, null, null));
        }

        Failures = failureList;
    }

    public static UseCaseResult<TData> NoContent(string? message = null) =>
        new(isSuccess: true, type: UseCaseResultType.NoContent, data: null, message: message, failures: null);

    public static UseCaseResult<TData> Created(TData data, string? message = null) =>
        new(isSuccess: true, type: UseCaseResultType.Created, data: data, message: message, failures: null);

    public static UseCaseResult<TData> Accepted(TData? data = null, string? message = null) =>
        new(isSuccess: true, type: UseCaseResultType.Accepted, data: data, message: message, failures: null);

    public static UseCaseResult<TData> Success(TData data, string? message = null) =>
        new(isSuccess: true, type: UseCaseResultType.Success, data: data, message: message, failures: null);

    public static UseCaseResult<TData> Unprocessable(string message, IReadOnlyList<FailureDetail>? failures = null) =>
        new(isSuccess: false, type: UseCaseResultType.Unprocessable, data: null, message: message, failures: failures ?? Array.Empty<FailureDetail>());

    public static UseCaseResult<TData> NotFound(string message, IReadOnlyList<FailureDetail>? failures = null) =>
        new(isSuccess: false, type: UseCaseResultType.NotFound, data: null, message: message, failures: failures ?? Array.Empty<FailureDetail>());

    public static UseCaseResult<TData> Failure(string message, IReadOnlyList<FailureDetail>? failures = null) =>
        new(isSuccess: false, type: UseCaseResultType.Failure, data: null, message: message, failures: failures ?? Array.Empty<FailureDetail>())    ;

}

