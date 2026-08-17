
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.DTOs.OrderItem;
using MusicStore.Application.Interfaces;
using MusicStore.Application.Interfaces.Services;
using MyStoreCore.ViewModels.Order;
using System.Security.Claims;

namespace MyStoreCore.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;
        private readonly IPaymentService _paymentService;

        public OrdersController(
            IOrderService orderService,
            ICartService cartService,
            ICouponService couponService,
            IPaymentService paymentService)
        {
            _orderService = orderService;
            _cartService = cartService;
            _couponService = couponService;
            _paymentService = paymentService;

        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }






        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();

            var cart = await _cartService.GetCartAsync(userId);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "سبد خرید شما خالی است.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart,
                DiscountAmount = 0,
                FinalPrice = cart.TotalPrice
            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            var userId = GetUserId();

            var cart = await _cartService.GetCartAsync(userId);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "سبد خرید شما خالی است.";

                return RedirectToAction(
                    "Index",
                    "Cart");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart,
                CouponCode = couponCode,
                DiscountAmount = 0,
                FinalPrice = cart.TotalPrice
            };

            var result = await _couponService.ValidateCouponAsync(
                couponCode,
                cart.TotalPrice);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    nameof(model.CouponCode),
                    result.Message);

                return View("Checkout", model);
            }

            model.DiscountAmount = result.Data.DiscountAmount;
            model.FinalPrice = result.Data.FinalPrice;

            return View("Checkout", model);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CheckoutViewModel model)
        {
            var userId = GetUserId();

            var cart = await _cartService.GetCartAsync(userId);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "سبد خرید شما خالی است.";

                return RedirectToAction(
                    "Index",
                    "Cart");
            }

            model.Cart = cart;

            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            var dto = new CreateOrderDto
            {
                Items = cart.Items
                    .Select(x => new CreateOrderItemDto
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity
                    })
                    .ToList(),

                ShippingInfo = model.ShippingInfo,

                CouponCode = model.CouponCode
            };

            var result = await _orderService.CreateOrderAsync(
                dto,
                userId);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    "",
                    result.Message);

                return View("Checkout", model);
            }

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = result.Data.Id
                });
        }




        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();

            var result = await _orderService.GetOrderByIdAsync( id,userId);

            if (!result.Success)
            {
                return NotFound();
            }

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserId();

            var result = await _orderService.GetUserOrdersAsync(userId);

            if (!result.Success || result.Data == null)
            {
                return View(new List<OrderDto>());
            }

            return View(result.Data);
        }


        [HttpGet]
        public async Task<IActionResult> Pay(int id)
        {
            var userId = GetUserId();

            var result = await _paymentService.CreatePaymentAsync(
                id,
                userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            return RedirectToAction(
                "Index",
                "Payment",
                new
                {
                    transactionId = result.Data.TransactionId
                });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();

            var result = await _orderService.CancelOrderAsync(
                id,
                userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            TempData["Success"] = "سفارش با موفقیت لغو شد.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }







        //[HttpGet]
        //public async Task<IActionResult> DeleteMyTestOrders()
        //{
        //    var userId = GetUserId();

        //    var result = await _orderService.DeleteUserOrdersAsync(userId);

        //    if (!result.Success)
        //    {
        //        TempData["Error"] = result.Message;
        //    }
        //    else
        //    {
        //        TempData["Success"] = result.Message;
        //    }

        //    return RedirectToAction(nameof(MyOrders));
        //}
    }
}

