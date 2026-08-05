using MusicStore.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class Coupon
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;

        public DiscountType DiscountType { get; set; }

        public decimal Value { get; set; }

        public decimal? MinimumOrderAmount { get; set; }

        public int UsageLimit { get; set; }

        public int UsedCount { get; set; }

        public DateTime ExpireDate { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
