using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Users;
using MusicStore.Application.Interfaces;
using MusicStore.Infrastructure.Identity;
using MyStoreCore.ViewModels;

namespace MyStoreCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(
            IUserService userService,
            UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }





        public async Task<IActionResult> Index(UserFilterDto filter)
        {
            var users = await _userService.GetUsersAsync(filter);

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new ChangeRoleViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                CurrentRole = roles.FirstOrDefault() ?? ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string id, string newRole)
        {
            var result = await _userService.ChangeRoleAsync(id, newRole);

            if (result)
            {
                TempData["Success"] = "نقش کاربر با موفقیت تغییر کرد.";
            }
            else
            {
                TempData["Error"] = "تغییر نقش انجام نشد.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            var result = await _userService.DeleteUserAsync(id);

            if (result)
            {
                TempData["Success"] = "کاربر با موفقیت حذف شد.";
            }
            else
            {
                TempData["Error"] = "حذف کاربر انجام نشد.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}