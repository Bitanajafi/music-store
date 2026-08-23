using MusicStore.Application.DTOs.Brand;
using MusicStore.Application.Interfaces;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Domain.Entities;
using MusicStore.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Infrastructure.Services
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        public BrandService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }




        public async Task<List<BrandDto>> GetAllAsync()
        {
            var brand = await _unitOfWork.Repository<Brand>().GetAllAsync();
            return brand.Select(x => new BrandDto
            {
                Id = x.Id,
                Name = x.Name,
                Country = x.Country,
                ProductCount = x.Products.Count

            }).ToList();
        }
        public async Task<BrandDto?> GetByIdAsync(int id)
        {
           var brand=await _unitOfWork.Repository<Brand>().GetByIdAsync(id);
            if (brand == null) 
            {
                return null;
            }
            return new BrandDto
            {
                Id = id,
                Name = brand.Name,
                Country = brand.Country,
                ProductCount = brand.Products.Count
            };
        }
        public async Task<bool> CreateAsync(CreateBrandDto dto)
        {
            var exists= await _unitOfWork.Repository<Brand>().AnyAsync(x=>x.Name==dto.Name);
            if (exists) 
                return false;

            var brand = new Brand
            {
                Name = dto.Name,
                Country = dto.Country,
            };
            await _unitOfWork.Repository<Brand>().AddAsync(brand);
            await _unitOfWork.SaveAsync();
            return true;

        }
        public async Task<UpdateBrandDto?> GetForUpdateAsync(int id)
        {
            var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(id);
            if(brand == null)
                { return null; }
            return new UpdateBrandDto
            {
                Id = id,
                Name = brand.Name,
                Country = brand.Country,
            };
        }

        public async Task<bool> UpdateAsync(UpdateBrandDto dto)
        {
            var brand= await _unitOfWork.Repository<Brand>().GetByIdAsync(dto.Id);
            if (brand == null)
                return false;

            brand.Name = dto.Name;
            brand.Country = dto.Country;

            await _unitOfWork.Repository<Brand>().UpdateAsync(brand);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(id);
            if(brand == null)
                { return false; }

            var products = await _unitOfWork.Repository<Product>().FindAsync(x => x.BrandId == id);
            if (products.Any())
                return false;
            await _unitOfWork.Repository<Brand>().DeleteAsync(brand);
            await _unitOfWork.SaveAsync();

            return true;

        }

        public async Task<List<BrandDto>> SearchAsync(string? search)
        {
            var brands = await _unitOfWork.Repository<Brand>().GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                brands = brands
                    .Where(x => x.Name.Contains(search))
                    .ToList();
            }

            return brands.Select(x => new BrandDto
            {
                Id = x.Id,
                Name = x.Name,
                Country = x.Country,
                ProductCount = x.Products.Count
            }).ToList();
        }
    }
}
