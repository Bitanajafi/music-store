using MusicStore.Domain.Enum;

namespace MusicStore.Application.DTOs.Order
{
    public class OrderHistoryDto
    {
        public OrderStatus? OldStatus { get; set; }


        public OrderStatus NewStatus { get; set; }


        public string? Description { get; set; }


        public string? ChangedBy { get; set; }


        public DateTime CreatedAt { get; set; }
    }
}