using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;

namespace Nstech.Challenge.OrderServices.Infrastructure.Database.EfCore;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> that delegates to <see cref="AppDbContext"/>.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
