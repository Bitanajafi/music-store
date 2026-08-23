
using AutoMapper;
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Coupon;
using MusicStore.Application.DTOs.Order;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Application.Interfaces.Services;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Enum;

namespace MusicStore.Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Coupon> _couponRepository;
        private readonly IMapper _mapper;

        public CouponService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _couponRepository = _unitOfWork.Repository<Coupon>();
            _mapper = mapper;
        }
        private static string NormalizeCode(string code)
        {
            return code.Trim().ToUpperInvariant();
        }




        public async Task<ServiceResult<CouponResultDto>> ValidateCouponAsync(string couponCode,decimal subTotal)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                return ServiceResult<CouponResultDto>
                    .Fail("کد تخفیف را وارد کنید.");
            }

            if (subTotal < 0)
            {
                return ServiceResult<CouponResultDto>
                    .Fail("مبلغ سفارش نامعتبر است.");
            }

            var code = NormalizeCode(couponCode);

            var coupon = await _couponRepository
                .GetFirstOrDefaultAsync(x => x.Code == code);

            if (coupon == null)
            {
                return ServiceResult<CouponResultDto>
                    .Fail("کد تخفیف یافت نشد.");
            }

            if (!coupon.IsActive)
            {
                return ServiceResult<CouponResultDto>
                    .Fail("کد تخفیف فعال نیست.");
            }

            if (coupon.ExpireDate <= DateTime.UtcNow)
            {
                return ServiceResult<CouponResultDto>
                    .Fail("اعتبار کد تخفیف تمام شده است.");
            }

            if (coupon.UsageLimit > 0 &&
                coupon.UsedCount >= coupon.UsageLimit)
            {
                return ServiceResult<CouponResultDto>
                    .Fail("ظرفیت استفاده از این کد تخفیف به پایان رسیده است.");
            }

            if (coupon.MinimumOrderAmount.HasValue &&
                subTotal < coupon.MinimumOrderAmount.Value)
            {
                return ServiceResult<CouponResultDto>
                    .Fail("مبلغ سفارش برای استفاده از این کد تخفیف کافی نیست.");
            }

            decimal discountAmount;

            if (coupon.DiscountType == DiscountType.Percentage)
            {
                discountAmount = subTotal * coupon.Value / 100;
            }
            else
            {
                discountAmount = coupon.Value;
            }

            if (discountAmount > subTotal)
            {
                discountAmount = subTotal;
            }

            var finalPrice = subTotal - discountAmount;

            return ServiceResult<CouponResultDto>.Ok(
                new CouponResultDto
                {
                    CouponId = coupon.Id,
                    CouponCode = coupon.Code,
                    DiscountAmount = discountAmount,
                    FinalPrice = finalPrice
                });
        }



        public async Task<ServiceResult<CouponDto>> CreateAsync(CreateCouponDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                return ServiceResult<CouponDto>.Fail(
                    "کد تخفیف الزامی است.");
            }

            if (dto.Code.Trim().Length > 50)
            {
                return ServiceResult<CouponDto>.Fail(
                    "کد تخفیف نمی‌تواند بیشتر از 50 کاراکتر باشد.");
            }

            if (dto.Value <= 0)
            {
                return ServiceResult<CouponDto>.Fail(
                    "مقدار تخفیف باید بیشتر از صفر باشد.");
            }

            if (dto.DiscountType == DiscountType.Percentage &&
                dto.Value > 100)
            {
                return ServiceResult<CouponDto>.Fail(
                    "درصد تخفیف نمی‌تواند بیشتر از 100 باشد.");
            }

            if (dto.MinimumOrderAmount.HasValue &&
                dto.MinimumOrderAmount.Value < 0)
            {
                return ServiceResult<CouponDto>.Fail(
                    "حداقل مبلغ سفارش نمی‌تواند منفی باشد.");
            }

            if (dto.UsageLimit < 0)
            {
                return ServiceResult<CouponDto>.Fail(
                    "محدودیت استفاده نمی‌تواند منفی باشد.");
            }

            if (dto.ExpireDate <= DateTime.UtcNow)
            {
                return ServiceResult<CouponDto>.Fail(
                    "تاریخ انقضا باید در آینده باشد.");
            }





            var code = NormalizeCode(dto.Code);

            var exists = await _couponRepository.AnyAsync(x => x.Code == code);

            if (exists)
            {
                return ServiceResult<CouponDto>.Fail(
                    "این کد تخفیف قبلاً ثبت شده است.");
            }

            var coupon = _mapper.Map<Coupon>(dto);

            coupon.Code = code;
            coupon.UsedCount = 0;
            coupon.CreatedAt = DateTime.UtcNow;

            await _couponRepository.AddAsync(coupon);
            await _unitOfWork.SaveAsync();

            var result = _mapper.Map<CouponDto>(coupon);

            return ServiceResult<CouponDto>.Ok(result, "کد تخفیف با موفقیت ایجاد شد.");
        }





        public async Task<ServiceResult<IEnumerable<CouponDto>>> GetAllAsync()
        {
            var coupons = await _couponRepository.GetAllAsync();

            var result = _mapper.Map<IEnumerable<CouponDto>>(coupons.OrderByDescending(x => x.CreatedAt));

            return ServiceResult<IEnumerable<CouponDto>>.Ok(result,"لیست کدهای تخفیف با موفقیت دریافت شد.");
        }




        public async Task<ServiceResult<CouponDto>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<CouponDto>.Fail("شناسه کد تخفیف نامعتبر است.");
            }



            var coupon = await _couponRepository.GetByIdAsync(id);

            if (coupon == null)
            {
                return ServiceResult<CouponDto>.Fail("کد تخفیف مورد نظر پیدا نشد.");
            }



            var result = _mapper.Map<CouponDto>(coupon);

            return ServiceResult<CouponDto>.Ok(result,"کد تخفیف با موفقیت دریافت شد.");
        }



        public async Task<ServiceResult<CouponDto>> UpdateAsync(int id,CreateCouponDto dto)
        {
            if (id <= 0)
            {
                return ServiceResult<CouponDto>.Fail("شناسه کد تخفیف نامعتبر است.");
            }

            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                return ServiceResult<CouponDto>.Fail( "کد تخفیف الزامی است.");
            }

            if (dto.Code.Trim().Length > 50)
            {
                return ServiceResult<CouponDto>.Fail(
                    "کد تخفیف نمی‌تواند بیشتر از 50 کاراکتر باشد.");
            }

            if (dto.Value <= 0)
            {
                return ServiceResult<CouponDto>.Fail(
                    "مقدار تخفیف باید بیشتر از صفر باشد.");
            }

            if (dto.DiscountType == DiscountType.Percentage &&
                dto.Value > 100)
            {
                return ServiceResult<CouponDto>.Fail(
                    "درصد تخفیف نمی‌تواند بیشتر از 100 باشد.");
            }

            if (dto.MinimumOrderAmount.HasValue &&
                dto.MinimumOrderAmount.Value < 0)
            {
                return ServiceResult<CouponDto>.Fail(
                    "حداقل مبلغ سفارش نمی‌تواند منفی باشد.");
            }

            if (dto.UsageLimit < 0)
            {
                return ServiceResult<CouponDto>.Fail(
                    "محدودیت استفاده نمی‌تواند منفی باشد.");
            }

            if (dto.ExpireDate <= DateTime.UtcNow)
            {
                return ServiceResult<CouponDto>.Fail(
                    "تاریخ انقضا باید در آینده باشد.");
            }




            var coupon = await _couponRepository.GetByIdAsync(id);

            if (coupon == null)
            {
                return ServiceResult<CouponDto>.Fail(
                    "کد تخفیف مورد نظر پیدا نشد.");
            }



            var code = NormalizeCode(dto.Code);

            var duplicateCode = await _couponRepository.AnyAsync(x => x.Code == code && x.Id != id);

            if (duplicateCode)
            {
                return ServiceResult<CouponDto>.Fail("این کد تخفیف قبلاً ثبت شده است.");
            }

            _mapper.Map(dto, coupon);

            coupon.Code = code;
            coupon.UpdatedAt = DateTime.UtcNow;

            await _couponRepository.UpdateAsync(coupon);
            await _unitOfWork.SaveAsync();

            var result = _mapper.Map<CouponDto>(coupon);

            return ServiceResult<CouponDto>.Ok(result,"کد تخفیف با موفقیت ویرایش شد.");
        }





        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<bool>.Fail("شناسه کد تخفیف نامعتبر است.");
            }

            var coupon = await _couponRepository.GetByIdAsync(id);

            if (coupon == null)
            {
                return ServiceResult<bool>.Fail(
                    "کد تخفیف مورد نظر پیدا نشد.");
            }

            await _couponRepository.DeleteAsync(coupon);
            await _unitOfWork.SaveAsync();

            return ServiceResult<bool>.Ok(true,
                "کد تخفیف با موفقیت حذف شد.");
        }





        public async Task<ServiceResult<bool>> ToggleActiveAsync(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<bool>.Fail(
                    "شناسه کد تخفیف نامعتبر است.");
            }

            var coupon = await _couponRepository.GetByIdAsync(id);

            if (coupon == null)
            {
                return ServiceResult<bool>.Fail(
                    "کد تخفیف مورد نظر پیدا نشد.");
            }

            coupon.IsActive = !coupon.IsActive;
            coupon.UpdatedAt = DateTime.UtcNow;

            await _couponRepository.UpdateAsync(coupon);
            await _unitOfWork.SaveAsync();

            var message = coupon.IsActive
                ? "کد تخفیف فعال شد."
                : "کد تخفیف غیرفعال شد.";

            return ServiceResult<bool>.Ok(coupon.IsActive, message);
        }



        public async Task<List<CouponDto>> SearchAsync(string? search)
        {
            var coupons = await _unitOfWork
                .Repository<Coupon>()
                .GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                coupons = coupons
                    .Where(x => x.Code.Contains(search))
                    .ToList();
            }

            return coupons.Select(x => new CouponDto
            {
                Id = x.Id,
                Code = x.Code,
                DiscountType = x.DiscountType,
                Value = x.Value,
                MinimumOrderAmount = x.MinimumOrderAmount,
                UsageLimit = x.UsageLimit,
                UsedCount = x.UsedCount,
                ExpireDate = x.ExpireDate,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();
        }
    }
}

