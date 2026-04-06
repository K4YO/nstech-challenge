namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

/// <summary>
/// Base interface for use cases with input and output.
/// </summary>
/// <typeparam name="TInput">The input type for the use case</typeparam>
/// <typeparam name="TOutput">The output type for the use case</typeparam>
public interface IUseCase<in TInput, TOutput>
    where TOutput : Dto
{
    /// <summary>
    /// Executes the use case with the provided input.
    /// </summary>
    /// <param name="input">The input data for the use case</param>
    /// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
    /// <returns>The result of the use case execution</returns>
    Task<UseCaseResult<TOutput>> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
}
