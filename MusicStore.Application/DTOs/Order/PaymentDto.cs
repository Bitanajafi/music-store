using MusicStore.Domain.Enum;

namespace MusicStore.Application.DTOs.Order
{
    public class PaymentDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }


        public PaymentStatus Status { get; set; }


        public PaymentMethod Method { get; set; }


        public string? TransactionId { get; set; }


        public DateTime? PaidAt { get; set; }
        
    }
}