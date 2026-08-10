namespace MusicStore.Application.DTOs.Order
{
    public class CouponResultDto
    {
        public int CouponId { get; set; }

        public string CouponCode { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal FinalPrice { get; set; }
    }
}