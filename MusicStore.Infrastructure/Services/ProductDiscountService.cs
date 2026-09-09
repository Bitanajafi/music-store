
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.ProductDiscount;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Application.Interfaces.Services;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Enum;

namespace MusicStore.Application.Services
{
    public class ProductDiscountService : IProductDiscountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<ProductDiscount> _discountRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public ProductDiscountService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _discountRepository = _unitOfWork.Repository<ProductDiscount>();
            _productRepository = _unitOfWork.Repository<Product>();
            _mapper = mapper;
        }







        public async Task<ServiceResult<ProductDiscountDto>> CreateAsync(
            CreateProductDiscountDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SKU))
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "SKU محصول الزامی است.");
            }

            if (dto.Value <= 0)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "مقدار تخفیف باید بیشتر از صفر باشد.");
            }

            if (dto.DiscountType == DiscountType.Percentage &&
                dto.Value > 100)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "درصد تخفیف نمی‌تواند بیشتر از 100 باشد.");
            }

            if (dto.EndDate.HasValue &&
                dto.EndDate.Value <= dto.StartDate)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "تاریخ پایان تخفیف باید بعد از تاریخ شروع باشد.");
            }

            var sku = dto.SKU.Trim();

            var product = await _productRepository
                .GetFirstOrDefaultAsync(x => x.SKU == sku);

            if (product == null)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "محصول مورد نظر با این SKU پیدا نشد.");
            }

            if (dto.DiscountType == DiscountType.FixedAmount &&
                dto.Value > product.Price)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "مبلغ تخفیف نمی‌تواند بیشتر از قیمت محصول باشد.");
            }

            var exists = await _discountRepository.AnyAsync(x => x.ProductId == product.Id);

            if (exists)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "این محصول قبلاً دارای تخفیف است.");
            }

         
            var discount = _mapper.Map<ProductDiscount>(dto);

            discount.ProductId = product.Id;
            discount.CreatedAt = DateTime.UtcNow;

            discount.StartDate = dto.StartDate.ToUniversalTime();

            if (dto.EndDate.HasValue)
            {
                discount.EndDate = dto.EndDate.Value.ToUniversalTime();
            }

            await _discountRepository.AddAsync(discount);
            await _unitOfWork.SaveAsync();

            discount.Product = product;



            var result = _mapper.Map<ProductDiscountDto>(discount);

            return ServiceResult<ProductDiscountDto>.Ok(
                result,
                "تخفیف محصول با موفقیت ایجاد شد.");
        }





        public async Task<ServiceResult<IEnumerable<ProductDiscountDto>>> GetAllAsync()
        {
            var discounts = await _discountRepository
                .GetAllAsync(x => x.Product);

            var result = _mapper.Map<IEnumerable<ProductDiscountDto>>(
                discounts.OrderByDescending(x => x.CreatedAt));

            return ServiceResult<IEnumerable<ProductDiscountDto>>.Ok(
                result,
                "لیست تخفیف‌های محصولات با موفقیت دریافت شد.");
        }





        public async Task<ServiceResult<ProductDiscountDto>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "شناسه تخفیف محصول نامعتبر است.");
            }

            var discount = await _discountRepository
                .GetFirstOrDefaultAsync(
                    x => x.Id == id,
                    query => query.Include(x => x.Product));

            if (discount == null)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "تخفیف محصول مورد نظر پیدا نشد.");
            }

            var result = _mapper.Map<ProductDiscountDto>(discount);

            return ServiceResult<ProductDiscountDto>.Ok(
                result,
                "تخفیف محصول با موفقیت دریافت شد.");
        }




        public async Task<ServiceResult<ProductDiscountDto>> UpdateAsync(
            int id,
            UpdateProductDiscountDto dto)
        {
            if (id <= 0)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "شناسه تخفیف محصول نامعتبر است.");
            }

            if (string.IsNullOrWhiteSpace(dto.SKU))
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "SKU محصول الزامی است.");
            }

            if (dto.Value <= 0)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "مقدار تخفیف باید بیشتر از صفر باشد.");
            }

            if (dto.DiscountType == DiscountType.Percentage &&
                dto.Value > 100)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "درصد تخفیف نمی‌تواند بیشتر از 100 باشد.");
            }

            if (dto.EndDate.HasValue &&
                dto.EndDate.Value <= dto.StartDate)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "تاریخ پایان تخفیف باید بعد از تاریخ شروع باشد.");
            }

            var discount = await _discountRepository
                .GetFirstOrDefaultAsync(
                    x => x.Id == id,
                    query => query.Include(x => x.Product));

            if (discount == null)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "تخفیف محصول مورد نظر پیدا نشد.");
            }

            var sku = dto.SKU.Trim();

            var product = await _productRepository
                .GetFirstOrDefaultAsync(x => x.SKU == sku);

            if (product == null)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "محصول مورد نظر با این SKU پیدا نشد.");
            }

            if (dto.DiscountType == DiscountType.FixedAmount &&
                dto.Value > product.Price)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "مبلغ تخفیف نمی‌تواند بیشتر از قیمت محصول باشد.");
            }

            var duplicate = await _discountRepository
                .AnyAsync(x =>
                    x.ProductId == product.Id &&
                    x.Id != id);

            if (duplicate)
            {
                return ServiceResult<ProductDiscountDto>.Fail(
                    "این محصول قبلاً دارای تخفیف دیگری است.");
            }

            _mapper.Map(dto, discount);

            discount.ProductId = product.Id;
            discount.Product = product;
            discount.UpdatedAt = DateTime.UtcNow;

            await _discountRepository.UpdateAsync(discount);
            await _unitOfWork.SaveAsync();

            var result = _mapper.Map<ProductDiscountDto>(discount);

            return ServiceResult<ProductDiscountDto>.Ok(
                result,
                "تخفیف محصول با موفقیت ویرایش شد.");
        }







        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<bool>.Fail(
                    "شناسه تخفیف محصول نامعتبر است.");
            }

            var discount = await _discountRepository
                .GetByIdAsync(id);

            if (discount == null)
            {
                return ServiceResult<bool>.Fail(
                    "تخفیف محصول مورد نظر پیدا نشد.");
            }

            await _discountRepository.DeleteAsync(discount);
            await _unitOfWork.SaveAsync();

            return ServiceResult<bool>.Ok(
                true,
                "تخفیف محصول با موفقیت حذف شد.");
        }




        public async Task<ServiceResult<bool>> ToggleActiveAsync(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<bool>.Fail(
                    "شناسه تخفیف محصول نامعتبر است.");
            }

            var discount = await _discountRepository
                .GetByIdAsync(id);

            if (discount == null)
            {
                return ServiceResult<bool>.Fail(
                    "تخفیف محصول مورد نظر پیدا نشد.");
            }

            discount.IsActive = !discount.IsActive;
            discount.UpdatedAt = DateTime.UtcNow;

            await _discountRepository.UpdateAsync(discount);
            await _unitOfWork.SaveAsync();

            var message = discount.IsActive
                ? "تخفیف محصول فعال شد."
                : "تخفیف محصول غیرفعال شد.";

            return ServiceResult<bool>.Ok(
                discount.IsActive,
                message);
        }






        public async Task<ServiceResult<IEnumerable<ProductDiscountDto>>> SearchAsync(
            string? search,
            DiscountType? discountType,
            bool? isActive)
        {
            var discounts = await _discountRepository
                .GetAllAsync(x => x.Product);

            IEnumerable<ProductDiscount> query = discounts;

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Product.SKU.Contains(search) ||
                    x.Product.Name.Contains(search));
            }

            if (discountType.HasValue)
            {
                query = query.Where(x =>
                    x.DiscountType == discountType.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x =>
                    x.IsActive == isActive.Value);
            }

            var result = _mapper.Map<IEnumerable<ProductDiscountDto>>(
                query.OrderByDescending(x => x.CreatedAt));

            return ServiceResult<IEnumerable<ProductDiscountDto>>.Ok(
                result,
                "جستجوی تخفیف‌های محصولات با موفقیت انجام شد.");
        }
    }
}

