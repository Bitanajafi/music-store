
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Wishlist;

namespace MusicStore.Application.Interfaces.Wishlist
{
    public interface IWishlistService
    {
        Task<ServiceResult<List<WishlistDto>>> GetUserWishlistAsync(
            string userId);

        Task<ServiceResult<bool>> AddToWishlistAsync(
            string userId,
            int productId);

        Task<ServiceResult<bool>> RemoveFromWishlistAsync(
            string userId,
            int productId);

        Task<ServiceResult<bool>> IsInWishlistAsync(
            string userId,
            int productId);
    }
}

