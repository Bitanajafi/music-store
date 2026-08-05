using MusicStore.Domain.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public string ImageUrl { get; set; } = null!;

        public string? AltText { get; set; }

        public bool IsMain { get; set; }

        public int DisplayOrder { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;
    }
}
