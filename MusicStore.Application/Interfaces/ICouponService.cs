  
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Coupon;
using MusicStore.Application.DTOs.Order;

namespace MusicStore.Application.Interfaces.Services
{
    public interface ICouponService
    {
        Task<ServiceResult<CouponDto>> CreateAsync(CreateCouponDto dto);

        Task<ServiceResult<IEnumerable<CouponDto>>> GetAllAsync();

        Task<ServiceResult<CouponDto>> GetByIdAsync(int id);

        Task<ServiceResult<CouponDto>> UpdateAsync(int id, CreateCouponDto dto);

        Task<ServiceResult<bool>> DeleteAsync(int id);

        Task<ServiceResult<bool>> ToggleActiveAsync(int id);
        Task<ServiceResult<CouponResultDto>> ValidateCouponAsync(string couponCode,decimal subTotal);
    }
}

