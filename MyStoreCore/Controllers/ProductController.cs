using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Product;
using MusicStore.Application.Interfaces;


namespace MyStoreCore.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService= productService;
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


            return View(product);

        }
    }
}
