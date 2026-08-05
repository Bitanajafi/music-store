using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Persistence.Configurations
{
    public class ShippingInfoConfiguration : IEntityTypeConfiguration<ShippingInfo>
    {
        public void Configure(EntityTypeBuilder<ShippingInfo> builder)
        {
            builder.ToTable("ShippingInfos");


            builder.HasKey(x => x.Id);



            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);



            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);



            builder.Property(x => x.Address)
                .IsRequired()
                .HasMaxLength(500);



            builder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(100);



            builder.Property(x => x.PostalCode)
                .IsRequired()
                .HasMaxLength(20);



            builder.Property(x => x.TrackingNumber)
                .HasMaxLength(100);



            builder.Property(x => x.CreatedAt)
                .IsRequired();



            // Relationship

            builder.HasOne(x => x.Order)
                .WithOne(x => x.ShippingInfo)
                .HasForeignKey<ShippingInfo>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);




            builder.HasIndex(x => x.OrderId)
                .IsUnique();


            builder.HasIndex(x => x.TrackingNumber);
        }
    }
}