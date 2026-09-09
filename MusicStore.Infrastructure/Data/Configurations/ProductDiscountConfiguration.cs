using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Data.Configurations;

public class ProductDiscountConfiguration : IEntityTypeConfiguration<ProductDiscount>
{
    public void Configure(EntityTypeBuilder<ProductDiscount> builder)
    {
        builder.ToTable("ProductDiscounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiscountType)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithOne(x => x.Discount)
            .HasForeignKey<ProductDiscount>(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ProductId)
            .IsUnique();
    }
}