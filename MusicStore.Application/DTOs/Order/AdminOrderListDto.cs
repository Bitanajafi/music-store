using MusicStore.Domain.Enum;

namespace MusicStore.Application.DTOs.Order
{
    public class AdminOrderListDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public string CustomerName { get; set; } = null!;

        public string? CustomerPhone { get; set; }

        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; }

        public PaymentStatus? PaymentStatus { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}