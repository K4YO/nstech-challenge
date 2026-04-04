namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

public abstract class UseCase
{
}

public abstract class UseCase<TInput, TOutput>
{
    public abstract Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
}

