using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.ProductImage
{
    public class ProductImageDto
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = null!;

        public string? AltText { get; set; }

        public bool IsMain { get; set; }

        public int DisplayOrder { get; set; }

        public int ProductId { get; set; }
    }
}
