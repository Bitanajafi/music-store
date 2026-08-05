using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace MusicStore.Application.DTOs.ProductImage
{
    public class CreateProductImageDto
    {
        public int ProductId { get; set; }

        public IFormFile Image { get; set; } = null!;

        public string? AltText { get; set; }
    }
}
