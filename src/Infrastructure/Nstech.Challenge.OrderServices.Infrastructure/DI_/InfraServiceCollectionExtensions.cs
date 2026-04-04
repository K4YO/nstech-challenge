using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;
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
            // UseNpgsql is added dynamically through Npgsql.EntityFrameworkCore.PostgreSQL
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}





