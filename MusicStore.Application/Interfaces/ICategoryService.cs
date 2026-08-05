using MusicStore.Application.DTOs.Category;
using MusicStore.Domain.Entities;
using MusicStore.Application.ViewModels;

namespace MusicStore.Application.Interfaces
{
    public interface ICategoryService
    {
  
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<List<CategoryDto>> GetMainCategoriesAsync();
        Task<List<int>> GetCategoryTreeIdsAsync(int categoryId);
        Task<List<CategoryMenuViewModel>> GetCategoryMenuAsync();
        Task<bool> CreateAsync(CreateCategoryDto dto);

        Task<bool> UpdateAsync(UpdateCategoryDto dto);
        Task<UpdateCategoryDto?> GetForUpdateAsync(int id);

        Task<bool> DeleteAsync(int id);
    }
}