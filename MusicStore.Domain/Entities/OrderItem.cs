using MusicStore.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

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

    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;
}