using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Product;
using MusicStore.Application.DTOs.Review;
using MusicStore.Application.Interfaces;
using MyStoreCore.Models.Product;
using System.Security.Claims;

namespace MyStoreCore.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IReviewService _reviewService;

        public ProductController(
            IProductService productService,
            IReviewService reviewService)
        {
            _productService = productService;
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            ProductFilterDto filter)
        {
            var products = await _productService
                .GetFilteredAsync(filter);

            return View(products);
        }





        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService
                .GetProductDetailsAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(
                ClaimTypes.NameIdentifier
            )?.Value;

            var reviewsResult = await _reviewService
                .GetProductReviewsAsync(id, currentUserId);

            var viewModel = new ProductDetailsPageViewModel
            {
                Product = product,

                Reviews = reviewsResult.Success
                    ? reviewsResult.Data ?? new List<ReviewListDto>()
                    : new List<ReviewListDto>()
            };

            return View(viewModel);
        }
    }
}