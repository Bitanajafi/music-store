namespace MusicStore.Application.DTOs.Stock;

public class StockHistoryDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string SKU { get; set; } = null!;

    public int Quantity { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string CreatedByName { get; set; } = "سیستم";
}