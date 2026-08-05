using MusicStore.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetUsersAsync(UserFilterDto filter);
        Task<bool> ChangeRoleAsync(string userId, string newRole);
        Task<bool> DeleteUserAsync(string userId);
        
    }
}
