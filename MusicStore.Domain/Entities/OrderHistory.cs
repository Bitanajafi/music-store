using MusicStore.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class OrderHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        public OrderStatus? OldStatus { get; set; }

        public OrderStatus NewStatus { get; set; }

        public string? Description { get; set; }

        public string? ChangedBy { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}
