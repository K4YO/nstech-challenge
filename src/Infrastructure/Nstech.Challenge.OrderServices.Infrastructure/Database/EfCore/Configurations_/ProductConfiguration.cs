using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nstech.Challenge.OrderServices.AppCore.Domain.ProductAggregate;

namespace Nstech.Challenge.OrderServices.Infrastructure.Database.EfCore.Configurations_;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.AvailableQuantity)
            .IsRequired();

        builder.Property(p => p.ReservedQuantity)
            .IsRequired();

        builder.HasIndex(p => p.Sku)
            .IsUnique();
    }
}

