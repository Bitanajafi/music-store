using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.Interfaces.Services;
using System.Security.Claims;

namespace MusicStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AdminOrderFilterDto filter)
        {
            var result = await _orderService.GetAdminOrdersAsync(filter);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                return View(Enumerable.Empty<AdminOrderListDto>());
            }

            return View(result.Data);
        }



  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "اطلاعات وضعیت سفارش نامعتبر است.";

                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["Error"] =
                    "شناسه ادمین یافت نشد.";

                return RedirectToAction(nameof(Index));
            }

            var result = await _orderService
                .UpdateOrderStatusAsync(
                    id,
                    dto,
                    userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Index));
        }






        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _orderService.GetAdminOrderByIdAsync(id);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }
    }
}