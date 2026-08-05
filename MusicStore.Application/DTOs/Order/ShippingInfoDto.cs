namespace MusicStore.Application.DTOs.Order
{
    public class ShippingInfoDto
    {
        public string FullName { get; set; } = null!;


        public string PhoneNumber { get; set; } = null!;


        public string Address { get; set; } = null!;


        public string City { get; set; } = null!;


        public string PostalCode { get; set; } = null!;


        public string? TrackingNumber { get; set; }


        public DateTime? ShippedAt { get; set; }


        public DateTime? DeliveredAt { get; set; }
    }
}