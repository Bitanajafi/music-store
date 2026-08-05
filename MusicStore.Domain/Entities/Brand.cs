using MusicStore.Domain.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class Brand : BaseEntity
    {
        public string Name { get; set; } = null!;
        
        public string? Country { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
