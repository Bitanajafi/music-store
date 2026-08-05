using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Brand
{
    public class CreateBrandDto
    {
        public string Name { get; set; } = null!;
        public string? Country { get; set; }
    }
}
