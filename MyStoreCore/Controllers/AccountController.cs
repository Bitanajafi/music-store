using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Authentication;
using MusicStore.Application.Interfaces;
using MusicStore.Infrastructure.Identity;
using MusicStore.Infrastructure.Services;
using MusicStore.Web.ViewModels.Account;
using MyStoreCore.ViewModels;

namespace MyStoreCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }




        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var dto = new RegisterDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
            };
            var result = await _authService.RegisterAsync(dto);
            if (result == false)
            {
                ModelState.AddModelError("", "ثبت نام شما انجام نشد.");
                return View(model);
            }
            TempData["Success"] = "ثبت نام با موفقیت انجام شد.";
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var dto = new LoginDto
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = model.RememberMe
            };
            var result = await _authService.LoginAsync(dto);
            if (result == false)
            {
                ModelState.AddModelError("", "ایمیل یا پسسوورد شما اشتباه است");
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login");


            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                CreatedAt = user.CreatedAt
            };
            return View(model);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login");


            if (!ModelState.IsValid)
            {
                model.Email = user.Email!;
                model.CreatedAt = user.CreatedAt;

                return View(model);
            }


            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var passwordResult = await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword!,
                    model.NewPassword
                );


                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        switch (error.Code)
                        {
                            case "PasswordMismatch":
                                ModelState.AddModelError("", "رمز عبور فعلی اشتباه است.");
                                break;

                            case "PasswordTooShort":
                                ModelState.AddModelError("", "رمز جدید باید حداقل ۸ کاراکتر باشد.");
                                break;

                            case "PasswordRequiresDigit":
                                ModelState.AddModelError("", "رمز جدید باید حداقل یک عدد داشته باشد.");
                                break;

                            case "PasswordRequiresUpper":
                                ModelState.AddModelError("", "رمز جدید باید حداقل یک حرف بزرگ انگلیسی داشته باشد.");
                                break;

                            case "PasswordRequiresLower":
                                ModelState.AddModelError("", "رمز جدید باید حداقل یک حرف کوچک انگلیسی داشته باشد.");
                                break;

                            case "PasswordRequiresNonAlphanumeric":
                                ModelState.AddModelError("", "رمز جدید باید حداقل یک کاراکتر خاص مانند ! یا @ داشته باشد.");
                                break;

                            default:
                                ModelState.AddModelError("", error.Description);
                                break;
                        }
                    }


                    model.Email = user.Email!;
                    model.CreatedAt = user.CreatedAt;

                    return View(model);
                }
            }


            var updateResult = await _userManager.UpdateAsync(user);


            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                model.Email = user.Email!;
                model.CreatedAt = user.CreatedAt;

                return View(model);
            }


            TempData["Success"] = "اطلاعات با موفقیت ذخیره شد.";

            return RedirectToAction(nameof(Profile));
        }
    }
    
}




