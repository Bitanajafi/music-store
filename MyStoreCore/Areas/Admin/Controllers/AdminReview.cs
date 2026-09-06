using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Review;
using MusicStore.Application.Interfaces;
using MusicStore.Infrastructure.Services;

namespace MyStoreCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminReviewController : Controller
    {
        private readonly IProductService _productService;
        private readonly IReviewService _reviewService;

        public AdminReviewController(
            IReviewService reviewService
            ,IProductService productService)
        {
            _reviewService = reviewService;
            _productService = productService;
        }











        [HttpGet]
        public async Task<IActionResult> Index(
            AdminReviewFilterDto filter)
        {
            ViewBag.Products = await _productService
                .GetAllAsync();

            var result = await _reviewService
                .GetAdminReviewsAsync(filter);

            if (!result.Success)
            {
                TempData["ReviewError"] = result.Message;

                return View(
                    new List<AdminReviewListDto>());
            }

            return View(
                result.Data ?? new List<AdminReviewListDto>());
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus( UpdateReviewStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ReviewError"] = "اطلاعات وضعیت دیدگاه نامعتبر است.";

                return RedirectToAction(nameof(Index));
            }

            var adminId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;

            if (string.IsNullOrWhiteSpace(adminId))
            {
                return Challenge();
            }

            var result = await _reviewService.UpdateStatusAsync(dto,adminId);

            if (!result.Success)
            {
                TempData["ReviewError"] = result.Message;
            }
            else
            {
                TempData["ReviewSuccess"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReply(CreateReviewReplyDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ReviewError"] = "اطلاعات پاسخ نامعتبر است.";

                return RedirectToAction(nameof(Index));
            }

            var adminId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;

            if (string.IsNullOrWhiteSpace(adminId))
            {
                return Challenge();
            }

            var result = await _reviewService.CreateReplyAsync(
                dto,
                adminId);

            if (!result.Success)
            {
                TempData["ReviewError"] = result.Message;
            }
            else
            {
                TempData["ReviewSuccess"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}