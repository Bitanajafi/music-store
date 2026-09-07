
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.Interfaces.Wishlist;
using System.Security.Claims;

namespace MyStoreCore.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }





            
            [HttpGet]
            [Authorize]
            public async Task<IActionResult> Index()
            {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var result = await _wishlistService.GetUserWishlistAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return View(result.Data ?? new List<MusicStore.Application.DTOs.Wishlist.WishlistDto>());
        }





        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "شناسه محصول نامعتبر است."
                });
            }

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "کاربر شناسایی نشد."
                });
            }

            var result = await _wishlistService.AddToWishlistAsync(
                userId,
                productId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "شناسه محصول نامعتبر است."
                });
            }

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "کاربر شناسایی نشد."
                });
            }

            var result = await _wishlistService.RemoveFromWishlistAsync(
                userId,
                productId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> IsInWishlist(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "شناسه محصول نامعتبر است."
                });
            }

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "کاربر شناسایی نشد."
                });
            }

            var result = await _wishlistService.IsInWishlistAsync(
                userId,
                productId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }
    }
}
