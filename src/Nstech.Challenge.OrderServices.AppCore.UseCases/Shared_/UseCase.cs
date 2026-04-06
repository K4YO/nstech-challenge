using FluentValidation;
using Microsoft.Extensions.Logging;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

/// <summary>
/// Base class for use cases that provides standard execution behavior with automatic validation and structured logging.
/// </summary>
/// <typeparam name="TInput">The input type for the use case (must be a Dto)</typeparam>
/// <typeparam name="TOutput">The output type for the use case (typically UseCaseResult<T>)</typeparam>
public abstract class UseCase<TInput, TOutput> : IUseCase<TInput, TOutput>
    where TInput : Dto
    where TOutput : Dto
{
    private readonly IValidator<TInput> _validator;

    /// <summary>
    /// Logger instance available to derived use cases for business-specific logging.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the UseCase class.
    /// </summary>
    /// <param name="logger">The logger for the use case</param>
    /// <param name="validator">The validator for the input DTO</param>
    protected UseCase(ILogger logger, IValidator<TInput> validator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(validator);

        Logger = logger;
        _validator = validator;
    }

    /// <summary>
    /// Executes the use case with automatic validation of the input.
    /// Validates the input before calling ExecuteValidatedAsync method.
    /// </summary>
    /// <param name="input">The input data for the use case</param>
    /// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
    /// <returns>The result of the use case execution</returns>
    public async Task<UseCaseResult<TOutput>> ExecuteAsync(TInput input, CancellationToken cancellationToken = default)
    {
        var useCaseName = GetType().Name;
        Logger.LogDebug("{UseCase} started", useCaseName);

        var validationResult = await _validator.ValidateAsync(input, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            Logger.LogWarning("{UseCase} validation failed: {Errors}",
                useCaseName,
                string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

            return HandleValidationFailure(errors);
        }

        var result = await ExecuteValidatedAsync(input, cancellationToken);

        if (result.IsSuccess)
        {
            Logger.LogInformation("{UseCase} completed successfully with result {ResultType}", useCaseName, result.Type);
        }
        else
        {
            Logger.LogWarning("{UseCase} completed with {ResultType}: {Message}", useCaseName, result.Type, result.Message);
        }

        return result;
    }

    /// <summary>
    /// Hook that can be overridden to provide a custom result when validation fails.
    /// Default implementation returns an Unprocessable result.
    /// </summary>
    /// <param name="errors">Validation errors grouped by property</param>
    /// <returns>A UseCaseResult representing validation failure</returns>
    protected virtual UseCaseResult<TOutput> HandleValidationFailure(Dictionary<string, string[]> errors)
    {
        var failures = errors.SelectMany(kvp => kvp.Value.Select(msg => new FailureDetail(msg, kvp.Key))).ToList();
        return UseCaseResult<TOutput>.Unprocessable("Validation failed", failures);
    }

    /// <summary>
    /// Executes the validated use case logic.
    /// This method is called after input validation succeeds.
    /// </summary>
    /// <param name="input">The validated input data</param>
    /// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
    /// <returns>The result of the use case execution</returns>
    protected abstract Task<UseCaseResult<TOutput>> ExecuteValidatedAsync(TInput input, CancellationToken cancellationToken);
}




