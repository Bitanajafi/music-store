using MusicStore.Domain.Enum;

namespace MusicStore.Application.DTOs.ProductDiscount;

public class ProductDiscountDto
{
    public int Id { get; set; }

    public string SKU { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public DiscountType DiscountType { get; set; }

    public decimal Value { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}