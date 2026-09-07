
using MusicStore.Application.DTOs.Product;

namespace MusicStore.Application.DTOs.Wishlist
{
    public class WishlistDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public ProductDto Product { get; set; } = null!;
    }
}

