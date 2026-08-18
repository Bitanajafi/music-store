using System.ComponentModel.DataAnnotations;

namespace MusicStore.Application.DTOs.Stock;

public class ChangeStockDto
{
    [Required(ErrorMessage = "کد SKU محصول الزامی است.")]
    public string SKU { get; set; } = null!;

    [Range(1,int.MaxValue,ErrorMessage = "تعداد باید بیشتر از صفر باشد.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "دلیل تغییر موجودی الزامی است.")]
    public string Reason { get; set; } = null!;
}