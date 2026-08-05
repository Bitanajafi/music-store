namespace MusicStore.Application.DTOs.OrderItem
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }


        public string ProductName { get; set; } = null!;


        public int Quantity { get; set; }


        public decimal UnitPrice { get; set; }


        public decimal TotalPrice
        {
            get
            {
                return Quantity * UnitPrice;
            }
        }
    }
}