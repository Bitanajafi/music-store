using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;
using MusicStore.Infrastructure.Identity;

namespace MusicStore.Infrastructure.Data.Configurations
{
    public class ReviewReplyConfiguration : IEntityTypeConfiguration<ReviewReply>
    {
        public void Configure(EntityTypeBuilder<ReviewReply> builder)
        {
            builder.ToTable("ReviewReplies");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Comment)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.AdminId)
                .IsRequired();

            builder.HasOne(x => x.Review)
                .WithMany(x => x.Replies)
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ReviewId);

            builder.HasIndex(x => x.AdminId);
        }
    }
}