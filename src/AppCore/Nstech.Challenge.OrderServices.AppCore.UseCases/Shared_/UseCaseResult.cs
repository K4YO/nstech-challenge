namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

public class UseCaseResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public static UseCaseResult Success(string? message = null) =>
        new() { IsSuccess = true, Message = message };

    public static UseCaseResult Failure(string message, Dictionary<string, string[]>? errors = null) =>
        new() { IsSuccess = false, Message = message, Errors = errors };
}

public class UseCaseResult<TData>
{
    public bool IsSuccess { get; set; }
    public TData? Data { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public static UseCaseResult<TData> Success(TData data, string? message = null) =>
        new() { IsSuccess = true, Data = data, Message = message };

    public static UseCaseResult<TData> Failure(string message, Dictionary<string, string[]>? errors = null) =>
        new() { IsSuccess = false, Message = message, Errors = errors };
}

