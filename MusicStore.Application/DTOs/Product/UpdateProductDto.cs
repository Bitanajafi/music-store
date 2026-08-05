using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;


namespace MusicStore.Application.DTOs.Product
{
    public class UpdateProductDto
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


        public int BrandId { get; set; }




        public List<IFormFile>? Images { get; set; }
        public bool RemoveOldImages { get; set; }
    }
}
