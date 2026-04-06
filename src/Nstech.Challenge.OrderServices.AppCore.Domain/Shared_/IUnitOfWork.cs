namespace Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

/// <summary>
/// Coordinates the persistence of changes across multiple aggregates in a single atomic transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the underlying store as a single transaction.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
