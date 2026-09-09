using MusicStore.Domain.Enum;

namespace MusicStore.Application.DTOs.ProductDiscount;

public class CreateProductDiscountDto
{
    public string SKU { get; set; } = null!;

    public DiscountType DiscountType { get; set; }

    public decimal Value { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}