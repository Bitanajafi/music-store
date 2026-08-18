namespace MusicStore.Application.DTOs.Stock;

public class UpdateStockDto
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public string Reason { get; set; } = null!;
}