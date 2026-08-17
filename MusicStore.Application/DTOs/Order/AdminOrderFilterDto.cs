using MusicStore.Domain.Enum;

namespace MusicStore.Application.DTOs.Order
{
    public class AdminOrderFilterDto
    {
        public string? Search { get; set; }

        public OrderStatus? Status { get; set; }

        public PaymentStatus? PaymentStatus { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}