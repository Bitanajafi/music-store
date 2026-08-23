using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Brand;
using MusicStore.Application.Interfaces;
using MusicStore.Infrastructure.Services;

namespace MyStoreCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class Brand : Controller
    {
        private readonly IBrandService _brandService;
        public Brand(IBrandService brandService)
        {
            _brandService = brandService;
        }



        public async Task<IActionResult> Index(string? search)
        {
            var brands = await _brandService.SearchAsync(search);
            ViewBag.Search = search;
            return View(brands);
        }
       


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateBrandDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }


            var result = await _brandService.CreateAsync(dto);


            if (!result)
            {
                ModelState.AddModelError("", "This brand already exists.");
                return View(dto);
            }


            return RedirectToAction(nameof(Index));
        }





        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandService.GetForUpdateAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateBrandDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            var result = await _brandService.UpdateAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Unable to update brand.");
                return View(dto);
            }

            return RedirectToAction(nameof(Index));
        }






        public async Task<IActionResult> Delete(int id)
        {
            var result = await _brandService.DeleteAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "You can't delete this brand because it has products or doesn't exist.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
