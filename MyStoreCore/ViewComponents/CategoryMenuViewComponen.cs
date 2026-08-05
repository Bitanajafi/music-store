using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.Interfaces;

namespace MyStoreCore.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public CategoryMenuViewComponent(
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetCategoryMenuAsync();

            return View(categories);
        }
    }
}