using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string SKU { get; set; } = null!;

        public string? Description { get; set; }


        public decimal Price { get; set; }

        public decimal CostPrice { get; set; }

        public int StockQuantity { get; set; }

        public bool IsActive { get; set; }


        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;


        public int BrandId { get; set; }

        public string BrandName { get; set; } = null!;


        public string? MainImageUrl { get; set; }

        public int ImageCount { get; set; }
    }
}
