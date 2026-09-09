  
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.ProductDiscount;
using MusicStore.Application.Interfaces.Services;
using MusicStore.Domain.Enum;

namespace MusicStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ProductDiscountController : Controller
    {
        private readonly IProductDiscountService _productDiscountService;

        public ProductDiscountController(
            IProductDiscountService productDiscountService)
        {
            _productDiscountService = productDiscountService;
        }





        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            DiscountType? discountType,
            bool? isActive)
        {
            var result = await _productDiscountService.SearchAsync(
                search,
                discountType,
                isActive);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                ViewBag.Search = search;
                ViewBag.DiscountType = discountType;
                ViewBag.IsActive = isActive;

                return View(Enumerable.Empty<ProductDiscountDto>());
            }

            ViewBag.Search = search;
            ViewBag.DiscountType = discountType;
            ViewBag.IsActive = isActive;

            return View(result.Data);
        }






        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateProductDiscountDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _productDiscountService
                .CreateAsync(dto);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message);

                return View(dto);
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Index));
        }






        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _productDiscountService
                .GetByIdAsync(id);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Index));
            }

            var dto = new UpdateProductDiscountDto
            {
                SKU = result.Data.SKU,
                DiscountType = result.Data.DiscountType,
                Value = result.Data.Value,
                StartDate = result.Data.StartDate,
                EndDate = result.Data.EndDate,
                IsActive = result.Data.IsActive
            };

            ViewBag.ProductDiscountId = id;

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            UpdateProductDiscountDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductDiscountId = id;

                return View(dto);
            }

            var result = await _productDiscountService
                .UpdateAsync(id, dto);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message);

                ViewBag.ProductDiscountId = id;

                return View(dto);
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Index));
        }










        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productDiscountService
                .DeleteAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Index));
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var result = await _productDiscountService
                .ToggleActiveAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
