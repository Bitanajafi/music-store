using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Category
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = null!;

        public int DisplayOrder { get; set; }

        public int? ParentCategoryId { get; set; }
    }
}
