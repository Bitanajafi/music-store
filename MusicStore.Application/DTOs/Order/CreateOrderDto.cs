using MusicStore.Application.DTOs.OrderItem;

namespace MusicStore.Application.DTOs.Order
{
    public class CreateOrderDto
    {
        public List<CreateOrderItemDto> Items { get; set; } = new();

        public string? CouponCode { get; set; }

        public ShippingInfoDto ShippingInfo { get; set; } = null!;
    }
}