using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class ShippingInfo
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string FullName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Address { get; set; } = null!;


        public string City { get; set; } = null!;


        public string PostalCode { get; set; } = null!;


        public string? TrackingNumber { get; set; }


        public DateTime? ShippedAt { get; set; }


        public DateTime? DeliveredAt { get; set; }



        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;



        public Order Order { get; set; } = null!;
    }
}
    
