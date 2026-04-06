using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Nstech.Challenge.OrderServices.Infrastructure.Database.EfCore;
using Nstech.Challenge.OrderServices.Infrastructure.Database.EfCore.Repositories_;

namespace Nstech.Challenge.OrderServices.Infrastructure.DI_;

public static class InfraServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {   
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, b =>
                b.MigrationsAssembly("Nstech.Challenge.OrderServices.Infrastructure.Migrations.PostgreSQL"));
            // Ignore pending model changes warning in production
            // This is safe because migrations are applied before the app starts
            options.ConfigureWarnings(w => 
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}




























