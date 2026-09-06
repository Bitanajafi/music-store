using MusicStore.Application.DTOs.Product;
using MusicStore.Application.DTOs.Review;

namespace MyStoreCore.Models.Product
{
    public class ProductDetailsPageViewModel
    {
        public ProductDetailsDto Product { get; set; } = null!;

        public List<ReviewListDto> Reviews { get; set; } = new();
    }
}