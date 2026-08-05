using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Product
{
    public class ProductFilterDto
    {
        public string? Search { get; set; }

        public int? CategoryId { get; set; }

        public int? BrandId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public bool? IsActive { get; set; }
    }
}
