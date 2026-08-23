using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Category;
using MusicStore.Application.Interfaces;

namespace MyStoreCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }


        public async Task<IActionResult> Index(string? search)
        {
            var categories = await _categoryService.SearchAsync(search);

            ViewBag.Search = search;

            return View(categories);
        }



        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var parentCategories = await _categoryService.GetMainCategoriesAsync();

            ViewBag.ParentCategories = parentCategories;

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MainCategories = await _categoryService.GetMainCategoriesAsync();
                return View(dto);
            }
            var result = await _categoryService.CreateAsync(dto);

            if (!result)
            {
                ModelState.AddModelError("", "Category already exists.");

                ViewBag.MainCategories = await _categoryService.GetMainCategoriesAsync();
                return View(dto);
            }

            return RedirectToAction(nameof(Index));
        }




        [HttpGet]
        public async Task<IActionResult> Edit(int id) 
        {
            var dto = await _categoryService.GetForUpdateAsync(id);

            if (dto == null)
                return NotFound();

            ViewBag.MainCategories = await _categoryService.GetMainCategoriesAsync();

            return View(dto);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MainCategories = await _categoryService.GetMainCategoriesAsync();
                return View(dto);
            }

            var result = await _categoryService.UpdateAsync(dto);

            if (!result)
            {
                ModelState.AddModelError("", "Unable to update category.");

                ViewBag.MainCategories = await _categoryService.GetMainCategoriesAsync();

                return View(dto);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "This category cannot be deleted because it has sub categories or products.";
            }

            else
            {
                TempData["Success"] =
                    "Category deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}