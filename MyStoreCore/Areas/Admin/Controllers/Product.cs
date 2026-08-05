using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Product;
using MusicStore.Application.Interfaces;
using MusicStore.Infrastructure.Services;

namespace MyStoreCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class Product : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        public Product(IProductService productService, ICategoryService categoryService, IBrandService brandService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
        }





        public async Task<IActionResult> Index(ProductFilterDto filter)
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.Brands = await _brandService.GetAllAsync();

            var products = await _productService.GetFilteredAsync(filter);

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.Brands = await _brandService.GetAllAsync();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ViewBag.Brands = await _brandService.GetAllAsync();
                return View(dto);
            }
            var result = await _productService.CreateAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "کد SKU تکراری است.");
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ViewBag.Brands = await _brandService.GetAllAsync();
                return View(dto);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetForUpdateAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.Brands = await _brandService.GetAllAsync();

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ViewBag.Brands = await _brandService.GetAllAsync();
                return View(dto);
            }
            var result = await _productService.UpdateAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "ویرایش محصول انجام نشد.");
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ViewBag.Brands = await _brandService.GetAllAsync();
                return View(dto);
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}