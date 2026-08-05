using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");


            builder.HasKey(x => x.Id);


            //Property
            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(450);



            builder.Property(x => x.SubTotal)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.DiscountAmount)
                .HasPrecision(18, 2)
                .IsRequired();


            builder.Property(x => x.TotalPrice)
                .HasPrecision(18, 2)
                .IsRequired();



            builder.Property(x => x.Status)
                .IsRequired();



            builder.Property(x => x.CreatedAt)
                .IsRequired();




            //  Relationship
            builder.HasOne(x => x.Coupon)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CouponId)
                .OnDelete(DeleteBehavior.SetNull);


            builder.HasOne(x => x.Payment)
                .WithOne(x => x.Order)
                .HasForeignKey<Payment>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(x => x.ShippingInfo)
                .WithOne(x => x.Order)
                .HasForeignKey<ShippingInfo>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.OrderItems)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasMany(x => x.OrderHistory)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.UserId);
        }
    }
}