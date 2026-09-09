using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.ProductDiscount;
using MusicStore.Domain.Enum;

namespace MusicStore.Application.Interfaces.Services;

public interface IProductDiscountService
{
    Task<ServiceResult<ProductDiscountDto>> CreateAsync(CreateProductDiscountDto dto);

    Task<ServiceResult<IEnumerable<ProductDiscountDto>>> GetAllAsync();

    Task<ServiceResult<ProductDiscountDto>> GetByIdAsync(int id);

    Task<ServiceResult<ProductDiscountDto>> UpdateAsync(int id,UpdateProductDiscountDto dto);

    Task<ServiceResult<bool>> DeleteAsync(int id);

    Task<ServiceResult<bool>> ToggleActiveAsync(int id);

    Task<ServiceResult<IEnumerable<ProductDiscountDto>>> SearchAsync(
        string? search,
        DiscountType? discountType,
        bool? isActive);
}  