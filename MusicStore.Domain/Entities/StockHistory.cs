namespace MusicStore.Domain.Entities;

public class StockHistory
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? CreatedBy { get; set; }




    public Product Product { get; set; } = null!;
}