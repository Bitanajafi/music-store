using MusicStore.Application.DTOs.OrderItem;
using MusicStore.Domain.Enum;

namespace MusicStore.Application.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }


        public decimal SubTotal { get; set; }


        public decimal DiscountAmount { get; set; }


        public decimal TotalPrice { get; set; }


        public OrderStatus Status { get; set; }


        public DateTime CreatedAt { get; set; }


        public ShippingInfoDto? ShippingInfo { get; set; }


        public PaymentDto? Payment { get; set; }


        public List<OrderItemDto> Items { get; set; } = new();


        public List<OrderHistoryDto> History { get; set; } = new();
    }
}