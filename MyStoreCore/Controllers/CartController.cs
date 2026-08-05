using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Cart;
using MusicStore.Application.Interfaces;
using System.Security.Claims;

namespace MyStoreCore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;


        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();


            var cart = await _cartService
                .GetCartAsync(userId);


            return View(cart);
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var userId = GetUserId();


            var result = await _cartService
                .AddToCartAsync(userId, dto);



            if (!result)
            {
                TempData["Error"] = "امکان اضافه کردن محصول وجود ندارد.";

                return RedirectToAction("Index", "Product");
            }



            TempData["Success"] = "محصول به سبد اضافه شد.";


            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId,int quantity)
        {
            var userId = GetUserId();

            var result = await _cartService
                .UpdateQuantityAsync(
                    userId,
                    cartItemId,
                    quantity);



            if (!result)
            {
                TempData["Error"] = "تغییر تعداد انجام نشد.";
            }



            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = GetUserId();



            var result = await _cartService
                .RemoveItemAsync(
                    userId,
                    cartItemId);



            if (!result)
            {
                TempData["Error"] = "حذف محصول انجام نشد.";
            }



            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var userId = GetUserId();



            var result = await _cartService
                .ClearCartAsync(userId);



            if (!result)
            {
                TempData["Error"] = "سبد خرید خالی نشد.";
            }



            return RedirectToAction(nameof(Index));
        }
    }
}
