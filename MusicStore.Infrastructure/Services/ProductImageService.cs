using Humanizer;
using MusicStore.Application.DTOs.ProductImage;
using MusicStore.Application.Interfaces;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Infrastructure.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public ProductImageService(IUnitOfWork unitOfWork,IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }





        public async Task<List<ProductImageDto>> GetByProductIdAsync(int productId)
        {
            var images = await _unitOfWork
                .Repository<ProductImage>()
                .FindAsync(x => x.ProductId == productId);
            return images
               .OrderBy(x => x.DisplayOrder)
               .Select(x => new ProductImageDto
               {
                   Id = x.Id,
                   ImageUrl = x.ImageUrl,
                   AltText = x.AltText,
                   IsMain = x.IsMain,
                   DisplayOrder = x.DisplayOrder,
                   ProductId = x.ProductId

               })
               .ToList();
        }

        public async Task<bool> SetMainImageAsync(int id)
        {

            var image = await _unitOfWork
                .Repository<ProductImage>()
                .GetByIdAsync(id);



            if (image == null)
                return false;



            var images = await _unitOfWork
                .Repository<ProductImage>()
                .FindAsync(
                    x => x.ProductId == image.ProductId);



            foreach (var item in images)
            {
                item.IsMain = false;

                await _unitOfWork
                    .Repository<ProductImage>()
                    .UpdateAsync(item);
            }



            image.IsMain = true;



            await _unitOfWork
                .Repository<ProductImage>()
                .UpdateAsync(image);



            await _unitOfWork.SaveAsync();



            return true;
        }

        public async Task<bool> AddAsync(CreateProductImageDto dto)
        {
            var images =await _unitOfWork.Repository<ProductImage>().FindAsync(x=>x.ProductId == dto.ProductId);


            bool hasMainImage = images.Any(x => x.IsMain);


            var imageUrl = await _fileService
            .UploadProductImageAsync(
                dto.Image,
                dto.ProductId);



            var productImage = new ProductImage
            {
                ProductId = dto.ProductId,

                ImageUrl = imageUrl,

                AltText = dto.AltText,

                IsMain = !hasMainImage,

                DisplayOrder = images.Count() + 1
            };


            await _unitOfWork
                .Repository<ProductImage>()
                .AddAsync(productImage);



            await _unitOfWork.SaveAsync();


            return true;
        }
        public async Task<bool> UpdateAsync(UpdateProductImageDto dto)
        {
            var image = await _unitOfWork
                .Repository<ProductImage>()
                .GetByIdAsync(dto.Id);

            if (image == null)
                return false;

            if (dto.Image != null)
            {
                await _fileService.DeleteFileAsync(image.ImageUrl);

                var newUrl = await _fileService.UploadProductImageAsync(
                    dto.Image,
                    image.ProductId);

                image.ImageUrl = newUrl;
            }

            image.AltText = dto.AltText;
            image.DisplayOrder = dto.DisplayOrder;

            await _unitOfWork
                .Repository<ProductImage>()
                .UpdateAsync(image);

            await _unitOfWork.SaveAsync();

            if (dto.IsMain)
            {
                await SetMainImageAsync(image.Id);
            }

            return true;
        }
        public async Task<bool> DeleteAsync(int id)
        {

            var image = await _unitOfWork
                .Repository<ProductImage>()
                .GetByIdAsync(id);



            if (image == null)
                return false;



            await _fileService
                .DeleteFileAsync(image.ImageUrl);



            await _unitOfWork
                .Repository<ProductImage>()
                .DeleteAsync(image);



            await _unitOfWork.SaveAsync();



            return true;
        }


    }
}
