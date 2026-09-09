using Microsoft.EntityFrameworkCore;
using MusicStore.Application.DTOs.Product;
using MusicStore.Application.Interfaces;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ICategoryService _categoryService;

        public ProductService(
            IUnitOfWork unitOfWork,
            IFileService fileService,
            ICategoryService categoryService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _categoryService = categoryService;
        }


        public async Task<List<ProductDto>> GetAllAsync()
        {
            var products = await _unitOfWork
                .Repository<Product>()
                .GetAllAsync(
                    x => x.Category,
                    x => x.Brand,
                    x => x.Images,
                    x => x.Discount);

            var now = DateTime.UtcNow;

            return products.Select(x =>
            {
                var discountIsActive =
                    x.Discount != null &&
                    x.Discount.IsActive &&
                    x.Discount.StartDate <= now &&
                    (x.Discount.EndDate == null || x.Discount.EndDate >= now);

                decimal? finalPrice = null;

                if (discountIsActive)
                {
                    finalPrice = x.Discount!.DiscountType == MusicStore.Domain.Enum.DiscountType.Percentage
                        ? x.Price - (x.Price * x.Discount.Value / 100m)
                        : x.Price - x.Discount.Value;

                    if (finalPrice < 0)
                        finalPrice = 0;
                }

                return new ProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    SKU = x.SKU,
                    Description = x.Description,
                    Price = x.Price,
                    CostPrice = x.CostPrice,
                    StockQuantity = x.StockQuantity,
                    IsActive = x.IsActive,

                    CategoryId = x.CategoryId,
                    CategoryName = x.Category?.Name ?? "",

                    BrandId = x.BrandId,
                    BrandName = x.Brand?.Name ?? "",

                    MainImageUrl = x.Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl,
                    ImageCount = x.Images?.Count ?? 0,

                    HasDiscount = discountIsActive,
                    DiscountValue = discountIsActive ? x.Discount!.Value : null,
                    DiscountType = discountIsActive ? x.Discount!.DiscountType : null,
                    FinalPrice = finalPrice
                };
            }).ToList();
        }



        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _unitOfWork
                .Repository<Product>()
                .GetByIdAsync(
                    x => x.Id == id,
                    x => x.Category,
                    x => x.Brand,
                    x => x.Images,
                    x => x.Discount);

            if (product == null)
                return null;

            var now = DateTime.UtcNow;

            var discountIsActive =
                product.Discount != null &&
                product.Discount.IsActive &&
                product.Discount.StartDate <= now &&
                (product.Discount.EndDate == null || product.Discount.EndDate >= now);

            decimal? finalPrice = null;

            if (discountIsActive)
            {
                finalPrice = product.Discount!.DiscountType == MusicStore.Domain.Enum.DiscountType.Percentage
                    ? product.Price - (product.Price * product.Discount.Value / 100m)
                    : product.Price - product.Discount.Value;

                if (finalPrice < 0)
                    finalPrice = 0;
            }

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,

                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "",

                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? "",

                MainImageUrl = product.Images?.FirstOrDefault(x => x.IsMain)?.ImageUrl,
                ImageCount = product.Images?.Count ?? 0,

                HasDiscount = discountIsActive,
                DiscountValue = discountIsActive ? product.Discount!.Value : null,
                DiscountType = discountIsActive ? product.Discount!.DiscountType : null,
                FinalPrice = finalPrice
            };
        }


        public async Task<bool> CreateAsync(CreateProductDto dto)
        {
            var exists = await _unitOfWork
                .Repository<Product>()
                .AnyAsync(x => x.SKU == dto.SKU);

            if (exists)
                return false;

            var product = new Product
            {
                Name = dto.Name,
                SKU = dto.SKU,
                Slug = dto.Name
                    .Trim()
                    .ToLower()
                    .Replace(" ", "-"),
                Description = dto.Description,
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                StockQuantity = dto.StockQuantity,
                IsActive = dto.IsActive,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId
            };

            await _unitOfWork
                .Repository<Product>()
                .AddAsync(product);

            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<UpdateProductDto?> GetForUpdateAsync(int id)
        {
            var product = await _unitOfWork
                .Repository<Product>()
                .GetByIdAsync(id);

            if (product == null)
                return null;

            return new UpdateProductDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId
            };
        }

        public async Task<bool> UpdateAsync(UpdateProductDto dto)
        {
            var product = await _unitOfWork
                .Repository<Product>()
                .GetByIdAsync(dto.Id);

            if (product == null)
                return false;

            var exists = await _unitOfWork
                .Repository<Product>()
                .AnyAsync(x => x.SKU == dto.SKU && x.Id != dto.Id);

            if (exists)
                return false;

            product.Name = dto.Name;
            product.SKU = dto.SKU;
            product.Slug = dto.Name
                .Trim()
                .ToLower()
                .Replace(" ", "-");

            product.Description = dto.Description;
            product.Price = dto.Price;
            product.CostPrice = dto.CostPrice;
            product.StockQuantity = dto.StockQuantity;
            product.IsActive = dto.IsActive;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;

            await _unitOfWork
                .Repository<Product>()
                .UpdateAsync(product);

            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _unitOfWork
                .Repository<Product>()
                .GetByIdAsync(id);

            if (product == null)
                return false;

            await _unitOfWork
                .Repository<Product>()
                .DeleteAsync(product);

            await _unitOfWork.SaveAsync();

            await _fileService
                .DeleteProductFolderAsync(product.Id);

            return true;
        }
   
        public async Task<List<ProductDto>> GetFilteredAsync(ProductFilterDto filter)
        {
            var products = await _unitOfWork
                .Repository<Product>()
                .GetAllAsync(
                    x => x.Category,
                    x => x.Brand,
                    x => x.Images,
                    x => x.Discount
                );

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                products = products
                    .Where(x =>
                        x.Name.Contains(filter.Search) ||
                        x.SKU.Contains(filter.Search) ||
                        (x.Description != null &&
                         x.Description.Contains(filter.Search)))
                    .ToList();
            }

            if (filter.CategoryId.HasValue)
            {
                var categoryIds = await _categoryService
                    .GetCategoryTreeIdsAsync(filter.CategoryId.Value);

                products = products
                    .Where(x => categoryIds.Contains(x.CategoryId))
                    .ToList();
            }

            if (filter.BrandId.HasValue)
            {
                products = products
                    .Where(x => x.BrandId == filter.BrandId)
                    .ToList();
            }

            if (filter.MinPrice.HasValue)
            {
                products = products
                    .Where(x => x.Price >= filter.MinPrice)
                    .ToList();
            }

            if (filter.MaxPrice.HasValue)
            {
                products = products
                    .Where(x => x.Price <= filter.MaxPrice)
                    .ToList();
            }

            if (filter.IsActive.HasValue)
            {
                products = products
                    .Where(x => x.IsActive == filter.IsActive)
                    .ToList();
            }

            var now = DateTime.UtcNow;

            return products.Select(x =>
            {
                var discountIsActive =
                    x.Discount != null &&
                    x.Discount.IsActive &&
                    x.Discount.StartDate <= now &&
                    (x.Discount.EndDate == null || x.Discount.EndDate >= now);

                decimal? finalPrice = null;

                if (discountIsActive)
                {
                    finalPrice = x.Discount!.DiscountType == MusicStore.Domain.Enum.DiscountType.Percentage
                        ? x.Price - (x.Price * x.Discount.Value / 100m)
                        : x.Price - x.Discount.Value;

                    if (finalPrice < 0)
                        finalPrice = 0;
                }

                return new ProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    SKU = x.SKU,
                    Description = x.Description,
                    Price = x.Price,
                    CostPrice = x.CostPrice,
                    StockQuantity = x.StockQuantity,
                    IsActive = x.IsActive,

                    CategoryId = x.CategoryId,
                    CategoryName = x.Category?.Name ?? "",

                    BrandId = x.BrandId,
                    BrandName = x.Brand?.Name ?? "",

                    MainImageUrl = x.Images
                        .FirstOrDefault(i => i.IsMain)?.ImageUrl,

                    ImageCount = x.Images.Count,

                    HasDiscount = discountIsActive,
                    DiscountValue = discountIsActive ? x.Discount!.Value : null,
                    DiscountType = discountIsActive ? x.Discount!.DiscountType : null,
                    FinalPrice = finalPrice
                };
            }).ToList();
        }


        public async Task<ProductDetailsDto?> GetProductDetailsAsync(int id)
        {
            var product = await _unitOfWork
                .Repository<Product>()
                .GetFirstOrDefaultAsync(
                    x => x.Id == id,
                    query => query
                        .Include(x => x.Images)
                        .Include(x => x.Category)
                        .Include(x => x.Brand)
                        .Include(x => x.Discount)
                );

            if (product == null)
                return null;

            var now = DateTime.UtcNow;

            var discountIsActive =
                product.Discount != null &&
                product.Discount.IsActive &&
                product.Discount.StartDate <= now &&
                (product.Discount.EndDate == null || product.Discount.EndDate >= now);

            decimal? finalPrice = null;

            if (discountIsActive)
            {
                finalPrice = product.Discount!.DiscountType == MusicStore.Domain.Enum.DiscountType.Percentage
                    ? product.Price - (product.Price * product.Discount.Value / 100m)
                    : product.Price - product.Discount.Value;

                if (finalPrice < 0)
                    finalPrice = 0;
            }

            return new ProductDetailsDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,

                CategoryName = product.Category.Name,
                BrandName = product.Brand.Name,

                Images = product.Images
                    .Select(x => x.ImageUrl)
                    .ToList(),

                HasDiscount = discountIsActive,
                DiscountValue = discountIsActive ? product.Discount!.Value : null,
                DiscountType = discountIsActive ? product.Discount!.DiscountType : null,
                FinalPrice = finalPrice
            };
        }


    }
}