using MusicStore.Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();

        Task<ProductDto?> GetByIdAsync(int id);

        Task<bool> CreateAsync(CreateProductDto dto);

        Task<UpdateProductDto?> GetForUpdateAsync(int id);  

        Task<bool> UpdateAsync(UpdateProductDto dto);

        Task<bool> DeleteAsync(int id);
        Task<List<ProductDto>> GetFilteredAsync(ProductFilterDto filter);
        Task<ProductDetailsDto?> GetProductDetailsAsync(int id);
    }
}
