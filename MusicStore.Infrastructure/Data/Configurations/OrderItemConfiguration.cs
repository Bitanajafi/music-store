using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");


            builder.HasKey(x => x.Id);



            builder.Property(x => x.Quantity)
                .IsRequired();



            builder.Property(x => x.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();



            //  Relationship

            builder.HasOne(x => x.Order)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(x => x.Product)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);




            builder.HasIndex(x => x.OrderId);

            builder.HasIndex(x => x.ProductId);
        }
    }
}