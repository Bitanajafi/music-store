using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");


            builder.HasKey(x => x.Id);


            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);


            builder.Property(x => x.ImageUrl)
                .HasMaxLength(500);


            builder.Property(x => x.DisplayOrder)
                .IsRequired();


            builder.HasMany(x => x.Products)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(x => x.ParentCategory)
                .WithMany(x => x.SubCategories)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);



            builder.HasIndex(x => x.Name)
                .IsUnique();
        }
    }
}

