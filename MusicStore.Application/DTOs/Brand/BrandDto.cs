using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Brand
{
    public class BrandDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Country { get; set; }

        public int ProductCount { get; set; }
    }
}
