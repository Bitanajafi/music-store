using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;
using MusicStore.Infrastructure.Identity;

namespace MusicStore.Infrastructure.Data.Configurations
{
    public class ReviewLikeConfiguration : IEntityTypeConfiguration<ReviewLike>
    {
        public void Configure(EntityTypeBuilder<ReviewLike> builder)
        {
            builder.ToTable("ReviewLikes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.HasOne(x => x.Review)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.ReviewId,
                x.UserId
            })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        }
    }
}