using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Persistence.Configurations
{
    public class OrderHistoryConfiguration : IEntityTypeConfiguration<OrderHistory>
    {
        public void Configure(EntityTypeBuilder<OrderHistory> builder)
        {
            builder.ToTable("OrderHistories");


            builder.HasKey(x => x.Id);



            builder.Property(x => x.NewStatus)
                .IsRequired();



            builder.Property(x => x.Description)
                .HasMaxLength(500);



            builder.Property(x => x.ChangedBy)
                .HasMaxLength(450);



            builder.Property(x => x.CreatedAt)
                .IsRequired();




            builder.HasOne(x => x.Order)
                .WithMany(x => x.OrderHistory)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);



            builder.HasIndex(x => x.OrderId);

            builder.HasIndex(x => x.CreatedAt);
        }
    }
}