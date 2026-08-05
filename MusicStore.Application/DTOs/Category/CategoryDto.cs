using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int DisplayOrder { get; set; }

        public int? ParentCategoryId { get; set; }

        public string? ParentCategoryName { get; set; }

        public int ProductCount { get; set; }

        public int SubCategoryCount { get; set; }

    }
}
