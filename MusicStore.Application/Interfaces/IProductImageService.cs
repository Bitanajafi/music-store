using MusicStore.Application.DTOs.ProductImage;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.Interfaces
{
    public interface IProductImageService
    {
        Task<List<ProductImageDto>> GetByProductIdAsync(int productId);

        Task<bool> AddAsync(CreateProductImageDto dto);

        Task<bool> DeleteAsync(int id);

        Task<bool> SetMainImageAsync(int id);

        Task<bool> UpdateAsync(UpdateProductImageDto dto);
    }
}
