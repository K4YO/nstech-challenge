using Microsoft.EntityFrameworkCore;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;

namespace Nstech.Challenge.OrderServices.Infrastructure.Database.EfCore.Repositories_;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Update(product);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }
}
