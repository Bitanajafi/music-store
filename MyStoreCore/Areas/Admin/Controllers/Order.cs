using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.Interfaces.Services;

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
    }
}