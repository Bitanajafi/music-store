
using MusicStore.Domain.Enum;

namespace MusicStore.Application.DTOs.Coupon
{
    public class CreateCouponDto
    {
        public string Code { get; set; } = null!;

        public DiscountType DiscountType { get; set; }

        public decimal Value { get; set; }

        public decimal? MinimumOrderAmount { get; set; }

        public int UsageLimit { get; set; }

        public DateTime ExpireDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

