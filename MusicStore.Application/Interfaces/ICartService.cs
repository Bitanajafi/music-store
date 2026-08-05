using MusicStore.Application.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto?> GetCartAsync(string userId);


        Task<bool> AddToCartAsync(string userId, AddToCartDto dto);


        Task<bool> UpdateQuantityAsync(string userId,int cartItemId,int quantity);


        Task<bool> RemoveItemAsync(string userId,int cartItemId);

        Task<bool> ClearCartAsync(string userId);
    }
}
