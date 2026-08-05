using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.ProductImage;
using MusicStore.Application.Interfaces;

namespace MyStoreCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ProductImageController : Controller
    {
        private readonly IProductImageService _productImageService;


        public ProductImageController(
            IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }



        [HttpGet]
        public async Task<IActionResult> Index(int productId)
        {
            var images = await _productImageService
                .GetByProductIdAsync(productId);

            ViewBag.ProductId = productId;

            return View(images);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CreateProductImageDto dto)
        {
            if (dto.Image == null)
            {
                TempData["Error"] = "Please select an image.";
                return RedirectToAction(
                    nameof(Index),
                    new { productId = dto.ProductId });
            }


            var result = await _productImageService
                .AddAsync(dto);


            if (!result)
            {
                TempData["Error"] = "Image upload failed.";
            }
            else
            {
                TempData["Success"] = "Image uploaded successfully.";
            }


            return RedirectToAction(
                nameof(Index),
                new { productId = dto.ProductId });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int productId)
        {
            var result = await _productImageService
                .DeleteAsync(id);


            if (!result)
            {
                TempData["Error"] = "Image delete failed.";
            }
            else
            {
                TempData["Success"] = "Image deleted.";
            }


            return RedirectToAction(
                nameof(Index),
                new { productId });
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMainImage(int id, int productId)
        {
            var result = await _productImageService
                .SetMainImageAsync(id);


            if (!result)
            {
                TempData["Error"] = "Changing main image failed.";
            }
            else
            {
                TempData["Success"] = "Main image changed.";
            }


            return RedirectToAction(
                nameof(Index),
                new { productId });
        }

    }
}