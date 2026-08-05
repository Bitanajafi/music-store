using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Persistence.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("Coupons");


            builder.HasKey(x => x.Id);



            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);



            builder.Property(x => x.DiscountType)
                .IsRequired();



            builder.Property(x => x.Value)
                .HasPrecision(18, 2)
                .IsRequired();



            builder.Property(x => x.MinimumOrderAmount)
                .HasPrecision(18, 2);



            builder.Property(x => x.UsageLimit)
                .IsRequired();



            builder.Property(x => x.UsedCount)
                .IsRequired();



            builder.Property(x => x.ExpireDate)
                .IsRequired();



            builder.Property(x => x.IsActive)
                .IsRequired();



            builder.Property(x => x.CreatedAt)
                .IsRequired();



            builder.HasIndex(x => x.Code)
                .IsUnique();


            builder.HasIndex(x => x.IsActive);


            builder.HasIndex(x => x.ExpireDate);



            // Relationship

            builder.HasMany(x => x.Orders)
                .WithOne(x => x.Coupon)
                .HasForeignKey(x => x.CouponId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}