
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Wishlist;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Application.Interfaces.Wishlist;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WishlistService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }






        public async Task<ServiceResult<List<WishlistDto>>> GetUserWishlistAsync(
            string userId)
        {
            var wishlists = await _unitOfWork
                .Repository<Wishlist>()
                .GetAllAsync(
                    x => x.UserId == userId,
                    query => query
 
                    .Include(x => x.Product)
                    .ThenInclude(x => x.Images)
                    .Include(x => x.Product)
                    .ThenInclude(x => x.Category)
                    .Include(x => x.Product)
                    .ThenInclude(x => x.Brand)


                );

            var result = _mapper.Map<List<WishlistDto>>(wishlists);

            return ServiceResult<List<WishlistDto>>.Ok(result);
        }




        public async Task<ServiceResult<bool>> AddToWishlistAsync(
            string userId,
            int productId)
        {    
            var productExists = await _unitOfWork
                .Repository<Product>()
                .AnyAsync(x => x.Id == productId);

            if (!productExists)
            {
                return ServiceResult<bool>.Fail(
                    "محصول مورد نظر پیدا نشد.");
            }

            var alreadyExists = await _unitOfWork
                .Repository<Wishlist>()
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.ProductId == productId);

            if (alreadyExists)
            {
                return ServiceResult<bool>.Fail(
                    "این محصول قبلاً به لیست علاقه‌مندی‌ها اضافه شده است.");
            }

            var wishlist = new Wishlist
            {
                UserId = userId,
                ProductId = productId
            };

            await _unitOfWork
                .Repository<Wishlist>()
                .AddAsync(wishlist);

            await _unitOfWork.SaveAsync();

            return ServiceResult<bool>.Ok(
                true,
                "محصول با موفقیت به لیست علاقه‌مندی‌ها اضافه شد.");
        }





        public async Task<ServiceResult<bool>> RemoveFromWishlistAsync(
            string userId,
            int productId)
        {
            var wishlist = await _unitOfWork
                .Repository<Wishlist>()
                .GetFirstOrDefaultAsync(
                    x =>
                        x.UserId == userId &&
                        x.ProductId == productId);

            if (wishlist == null)
            {
                return ServiceResult<bool>.Fail(
                    "این محصول در لیست علاقه‌مندی‌ها وجود ندارد.");
            }

            await _unitOfWork
                .Repository<Wishlist>()
                .DeleteAsync(wishlist);

            await _unitOfWork.SaveAsync();

            return ServiceResult<bool>.Ok(
                true,
                "محصول با موفقیت از لیست علاقه‌مندی‌ها حذف شد.");
        }





        public async Task<ServiceResult<bool>> IsInWishlistAsync(
            string userId,
            int productId)
        {
            var exists = await _unitOfWork
                .Repository<Wishlist>()
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.ProductId == productId);

            return ServiceResult<bool>.Ok(exists);
        }
    }
}
