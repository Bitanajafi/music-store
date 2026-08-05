using Humanizer;
using MusicStore.Application.DTOs.Category;
using MusicStore.Application.Interfaces;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using MusicStore.Application.ViewModels;
namespace MusicStore.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(IUnitOfWork unitOfWork)
        {
          _unitOfWork = unitOfWork;
        }




        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var categories = await _unitOfWork
                .Repository<Category>()
                .GetAllAsync();


            return categories.Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                DisplayOrder = x.DisplayOrder,

                ParentCategoryId = x.ParentCategoryId,

                ParentCategoryName = x.ParentCategory?.Name,

                ProductCount = x.Products.Count,

                SubCategoryCount = x.SubCategories.Count

            }).ToList();
        }


        public async Task<List<CategoryDto>> GetMainCategoriesAsync()
        {
            var categories = await _unitOfWork
                .Repository<Category>()
                .FindAsync(x => x.ParentCategoryId == null);

            return categories.Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }

        public async Task<List<int>> GetCategoryTreeIdsAsync(int categoryId)
        {
            var ids = new List<int>
    {
        categoryId
    };

            var children = await _unitOfWork
                .Repository<Category>()
                .FindAsync(x => x.ParentCategoryId == categoryId);


            foreach (var child in children)
            {
                var childIds = await GetCategoryTreeIdsAsync(child.Id);

                ids.AddRange(childIds);
            }


            return ids;
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category=await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null)
                return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                DisplayOrder = category.DisplayOrder,

                ParentCategoryId = category.ParentCategoryId,

                ProductCount = category.Products.Count,

                SubCategoryCount = category.SubCategories.Count
            };
        }



        public async Task<bool> CreateAsync(CreateCategoryDto dto)
        {
            var exists = await _unitOfWork.Repository<Category>().AnyAsync(x => x.Name == dto.Name);
            if (exists) 
            {
                return false;
            }
            var category = new Category
            {
                Name = dto.Name,
                DisplayOrder = dto.DisplayOrder,

                ParentCategoryId = dto.ParentCategoryId
            };
            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.SaveAsync();
            return true;

        }





        public async Task<UpdateCategoryDto?> GetForUpdateAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
                if (category == null)
                    return null;

            return new UpdateCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                DisplayOrder = category.DisplayOrder,
                ParentCategoryId = category.ParentCategoryId
            };
        }

        public async Task<List<CategoryMenuViewModel>> GetCategoryMenuAsync()
        {
            var categories = await _unitOfWork
                .Repository<Category>()
                .GetAllAsync();


            var result = categories
                .Where(x => x.ParentCategoryId == null)
                .Select(category => new CategoryMenuViewModel
                {
                    Id = category.Id,
                    Name = category.Name,

                    Children = categories
                        .Where(sub => sub.ParentCategoryId == category.Id)
                        .Select(sub => new CategoryMenuViewModel
                        {
                            Id = sub.Id,
                            Name = sub.Name
                        })
                        .ToList()
                })
                .ToList();


            return result;
        }

        public async Task<bool> UpdateAsync(UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.Id);
            if (category == null)
                return false;

            category.Name = dto.Name;
            category.DisplayOrder = dto.DisplayOrder;
            if (dto.ParentCategoryId == dto.Id)
            {
                return false;
            }
            category.ParentCategoryId = dto.ParentCategoryId;

            await _unitOfWork.Repository<Category>().UpdateAsync(category);
            await _unitOfWork.SaveAsync();
            return true;
        }




        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>()
                .GetByIdAsync(id);

            if (category == null)
                return false;


            var children = await _unitOfWork.Repository<Category>()
                .FindAsync(x => x.ParentCategoryId == id);


            if (children.Any())
                return false;


            if (category.Products.Any())
                return false;


            await _unitOfWork.Repository<Category>()
                .DeleteAsync(category);

            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
