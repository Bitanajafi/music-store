using MusicStore.Domain.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; }



        public int? ParentCategoryId { get; set; }

        public Category? ParentCategory { get; set; }



        public ICollection<Category> SubCategories { get; set; }= new List<Category>();

        public ICollection<Product> Products { get; set; }= new List<Product>();
    }
}
