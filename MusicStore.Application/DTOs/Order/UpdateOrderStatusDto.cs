using MusicStore.Domain.Enum;

namespace MusicStore.Application.DTOs.Order
{
    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }

        public string? Description { get; set; }
    }
}