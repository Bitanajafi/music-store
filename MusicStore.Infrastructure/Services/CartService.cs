using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MusicStore.Application.DTOs.Cart;
using MusicStore.Application.Interfaces;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Domain.Entities;

namespace MusicStore.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<CartDto?> GetCartAsync(string userId)
        {
            var cart = await _unitOfWork
                .Repository<Cart>()
                .GetFirstOrDefaultAsync(
                    x => x.UserId == userId,
                    query => query
                        .Include(x => x.CartItems)
                        .ThenInclude(x => x.Product)
                        .ThenInclude(x => x.Images));


            if (cart == null)
            {
                return null;
            }


            return new CartDto
            {
                Id = cart.Id,

                Items = cart.CartItems.Select(item => new CartItemDto
                {
                    Id = item.Id,

                    ProductId = item.ProductId,

                    ProductName = item.Product.Name,

                    SKU = item.Product.SKU,

                    Quantity = item.Quantity,

                    UnitPrice = item.UnitPrice,

                    OriginalPrice = item.Product.Price,

                    ImageUrl = item.Product.Images
                        .FirstOrDefault(x => x.IsMain)
                        ?.ImageUrl

                }).ToList()
            };
        }




        public async Task<bool> AddToCartAsync(string userId, AddToCartDto dto)
        {
            if (dto.Quantity <= 0)
            {
                return false;
            }


            var cart = await _unitOfWork
                .Repository<Cart>()
                .GetFirstOrDefaultAsync(
                    x => x.UserId == userId,
                    query => query
                        .Include(x => x.CartItems));


            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };


                await _unitOfWork
                    .Repository<Cart>()
                    .AddAsync(cart);


                await _unitOfWork.SaveAsync();
            }




              var product = await _unitOfWork
             .Repository<Product>()
             .GetByIdAsync(
                 x => x.Id == dto.ProductId,
                 x => x.Discount);





            if (product == null)
            {
                return false;
            }



            if (!product.IsActive)
            {
                return false;
            }



            if (product.StockQuantity < dto.Quantity)
            {
                return false;
            }

            var finalPrice = product.Price;

            var now = DateTime.UtcNow;

            var discountIsActive =
                product.Discount != null &&
                product.Discount.IsActive &&
                product.Discount.StartDate <= now &&
                (product.Discount.EndDate == null || product.Discount.EndDate >= now);

            if (discountIsActive)
            {
                finalPrice = product.Discount!.DiscountType == MusicStore.Domain.Enum.DiscountType.Percentage
                    ? product.Price - (product.Price * product.Discount.Value / 100m)
                    : product.Price - product.Discount.Value;

                if (finalPrice < 0)
                    finalPrice = 0;
            }




            var existingItem = cart.CartItems
                .FirstOrDefault(x => x.ProductId == dto.ProductId);



            if (existingItem != null)
            {

                var newQuantity = existingItem.Quantity + dto.Quantity;


                if (newQuantity > product.StockQuantity)
                {
                    return false;
                }



                existingItem.Quantity = newQuantity;
                existingItem.UnitPrice = finalPrice;

                await _unitOfWork
                    .Repository<CartItem>()
                    .UpdateAsync(existingItem);

            }
            else
            {

                var cartItem = new CartItem
                {
                    CartId = cart.Id,

                    ProductId = product.Id,

                    Quantity = dto.Quantity,

                    UnitPrice = finalPrice
                };


                await _unitOfWork
                    .Repository<CartItem>()
                    .AddAsync(cartItem);

            }



            await _unitOfWork.SaveAsync();


            return true;
        }





        public async Task<bool> UpdateQuantityAsync(
        string userId,
        int cartItemId,
        int quantity)
            {
                if (quantity <= 0)
                {
                    return false;
                }
        
                var cartItem = await _unitOfWork
                    .Repository<CartItem>()
                    .GetFirstOrDefaultAsync(
                        x => x.Id == cartItemId &&
                             x.Cart.UserId == userId,
                        query => query
                            .Include(x => x.Cart));
        
                if (cartItem == null)
                {
                    return false;
                }
        
                var product = await _unitOfWork
                    .Repository<Product>()
                    .GetByIdAsync(
                        x => x.Id == cartItem.ProductId,
                        x => x.Discount);
        
                if (product == null)
                {
                    return false;
                }
        
                if (!product.IsActive)
                {
                    return false;
                }
        
                if (product.StockQuantity < quantity)
                {
                    return false;
                }
        
                var finalPrice = product.Price;
        
                var now = DateTime.UtcNow;
        
                var discountIsActive =
                    product.Discount != null &&
                    product.Discount.IsActive &&
                    product.Discount.StartDate <= now &&
                    (product.Discount.EndDate == null || product.Discount.EndDate >= now);
        
                if (discountIsActive)
                {
                    finalPrice = product.Discount!.DiscountType == MusicStore.Domain.Enum.DiscountType.Percentage
                        ? product.Price - (product.Price * product.Discount.Value / 100m)
                        : product.Price - product.Discount.Value;
        
                    if (finalPrice < 0)
                    {
                        finalPrice = 0;
                    }
                }
        
                cartItem.Quantity = quantity;
                cartItem.UnitPrice = finalPrice;
        
                await _unitOfWork
                    .Repository<CartItem>()
                    .UpdateAsync(cartItem);
        
                await _unitOfWork.SaveAsync();
        
                return true;
            }





        public async Task<bool> RemoveItemAsync(
            string userId,
            int cartItemId)
        {


            var cartItem = await _unitOfWork
                .Repository<CartItem>()
                .GetFirstOrDefaultAsync(
                    x => x.Id == cartItemId &&
                         x.Cart.UserId == userId,

                    query => query
                        .Include(x => x.Cart));



            if (cartItem == null)
            {
                return false;
            }



            await _unitOfWork
                .Repository<CartItem>()
                .DeleteAsync(cartItem);



            await _unitOfWork.SaveAsync();



            return true;
        }





        public async Task<bool> ClearCartAsync(string userId)
        {

            var cart = await _unitOfWork
                .Repository<Cart>()
                .GetFirstOrDefaultAsync(
                    x => x.UserId == userId,

                    query => query
                        .Include(x => x.CartItems));



            if (cart == null)
            {
                return false;
            }



            if (!cart.CartItems.Any())
            {
                return true;
            }



            foreach (var item in cart.CartItems)
            {
                await _unitOfWork
                    .Repository<CartItem>()
                    .DeleteAsync(item);
            }



            await _unitOfWork.SaveAsync();



            return true;
        }

    }
}