using System;
using System.Collections.Generic;
using System.Text;

    namespace MusicStore.Application.DTOs.Product
    {
        public class ProductDetailsDto
        {
            public int Id { get; set; }

            public string Name { get; set; } = null!;

            public string SKU { get; set; } = null!;


            public string? Description { get; set; }


            public decimal Price { get; set; }


            public int StockQuantity { get; set; }


            public bool IsActive { get; set; }



            public string CategoryName { get; set; } = null!;


            public string BrandName { get; set; } = null!;


            public List<string> Images { get; set; } = new();


            public List<ProductDto> RelatedProducts { get; set; } = new();

        }
    }

