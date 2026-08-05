using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");


            builder.HasKey(x => x.Id);



            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();



            builder.Property(x => x.Status)
                .IsRequired();



            builder.Property(x => x.Method)
                .IsRequired();



            builder.Property(x => x.TransactionId)
                .HasMaxLength(200);



            builder.Property(x => x.CreatedAt)
                .IsRequired();



            //  Relationship

            builder.HasOne(x => x.Order)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);



          

            builder.HasIndex(x => x.OrderId)
                .IsUnique();


            builder.HasIndex(x => x.TransactionId);
        }
    }
}