using MusicStore.Application.DTOs.Brand;

namespace MusicStore.Application.Interfaces
{
    public interface IBrandService
    {
        Task<List<BrandDto>> GetAllAsync();


        Task<BrandDto?> GetByIdAsync(int id);


        Task<bool> CreateAsync(CreateBrandDto dto);


        Task<UpdateBrandDto?> GetForUpdateAsync(int id);


        Task<bool> UpdateAsync(UpdateBrandDto dto);


        Task<bool> DeleteAsync(int id);
    }
}