using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.ProductImage
{
    public class UpdateProductImageDto
    {
        public int Id { get; set; }

        public IFormFile? Image { get; set; }

        public string? AltText { get; set; }

        public bool IsMain { get; set; }

        public int DisplayOrder { get; set; }
    }
}
