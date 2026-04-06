using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nstech.Challenge.OrderServices.AppCore.Domain.OrderAggregate;

namespace Nstech.Challenge.OrderServices.Infrastructure.Database.EfCore.Configurations_;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Id)
            .ValueGeneratedNever();

        builder.Property(oi => oi.SequenceNumber)
            .ValueGeneratedOnAdd();

        builder.Property(oi => oi.ProductId)
            .IsRequired();

        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        builder.Property(oi => oi.CreatedAt)
            .IsRequired();

        builder.Property(oi => oi.UpdatedAt)
            .IsRequired();

        builder.Property(oi => oi.Version)
            .IsRowVersion();
    }
}
