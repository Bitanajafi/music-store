using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicStore.Application.DTOs.Enums;
using MusicStore.Application.DTOs.Users;
using MusicStore.Application.Interfaces;
using MusicStore.Infrastructure.Identity;

namespace MusicStore.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }





        public async Task<List<UserDto>> GetUsersAsync(UserFilterDto filter)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x =>
                    x.FirstName.Contains(filter.Search) ||
                    x.LastName.Contains(filter.Search) ||
                    x.Email!.Contains(filter.Search));
            }


            if (filter.Sort == UserSortType.Oldest)
            {
                query = query.OrderBy(x => x.CreatedAt);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            var users = await query.ToListAsync();

            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive,
                    Role = roles.FirstOrDefault() ?? "Customer"
                });
            }

            return result;
        }

        public async Task<bool> ChangeRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;


            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("SuperAdmin"))
            {
                return false;
            }


            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            var result = await _userManager.AddToRoleAsync(user, newRole);

            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;


            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("SuperAdmin"))
            {
                return false;
            }



            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded;
        }

       
    }
}