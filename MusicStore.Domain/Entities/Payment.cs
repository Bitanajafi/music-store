using MusicStore.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }


        public int OrderId { get; set; }


        public decimal Amount { get; set; }


        public PaymentStatus Status { get; set; }


        public PaymentMethod Method { get; set; }


        public string? TransactionId { get; set; }


        public DateTime? PaidAt { get; set; }


        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;


        public Order Order { get; set; } = null!;
    }
}

