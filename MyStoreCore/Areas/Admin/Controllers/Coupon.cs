
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Coupon;
using MusicStore.Application.Interfaces.Services;

namespace MusicStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }






        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                var result = await _couponService.GetAllAsync();

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(Enumerable.Empty<CouponDto>());
                }

                ViewBag.Search = search;

                return View(result.Data);
            }

            var coupons = await _couponService.SearchAsync(search);

            ViewBag.Search = search;

            return View(coupons);
        }






        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCouponDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _couponService.CreateAsync(dto);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(dto);
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Index));
        }




        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _couponService.GetByIdAsync(id);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var dto = new CreateCouponDto
            {
                Code = result.Data.Code,
                DiscountType = result.Data.DiscountType,
                Value = result.Data.Value,
                MinimumOrderAmount = result.Data.MinimumOrderAmount,
                UsageLimit = result.Data.UsageLimit,
                ExpireDate = result.Data.ExpireDate,
                IsActive = result.Data.IsActive
            };

            ViewBag.CouponId = id;

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,CreateCouponDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CouponId = id;
                return View(dto);
            }

            var result = await _couponService.UpdateAsync(id, dto);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                ViewBag.CouponId = id;
                return View(dto);
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _couponService.DeleteAsync(id);

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
            var result = await _couponService.ToggleActiveAsync(id);

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

