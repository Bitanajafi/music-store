using MusicStore.Domain.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class Wishlist : BaseEntity
    {
        public string UserId { get; set; } = null!;

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;
    }
}
