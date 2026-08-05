using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
        .IsRequired()
        .HasMaxLength(500);

        builder.Property(x => x.AltText)
            .HasMaxLength(250);

        builder.Property(x => x.IsMain)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ProductId);

        builder.HasIndex(x => new { x.ProductId, x.DisplayOrder });

        builder.HasIndex(x => new { x.ProductId, x.IsMain })
         .HasFilter("[IsMain] = 1")
         .IsUnique();
    }
}
