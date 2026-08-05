using MusicStore.Domain.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; }

        public string Comment { get; set; } = null!;

        public bool IsApproved { get; set; }

        public string UserId { get; set; } = null!;

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;
    }
}
