using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using MusicStore.Application.DTOs.Authentication;
using MusicStore.Application.Interfaces;
using MusicStore.Infrastructure.Identity;

namespace MusicStore.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    public AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        var userExists = await _userManager.FindByEmailAsync(dto.Email);

        if (userExists != null)
        {
            return false;
        }
        var user = new ApplicationUser
        {
            UserName=dto.Email,
            Email=dto.Email,
            FirstName=dto.FirstName,
            LastName=dto.LastName,
            IsActive=true
        };
        var saveUser= await _userManager.CreateAsync(user,dto.Password);
        if(!saveUser.Succeeded)
        {
            return false;
        }
        await _signInManager.SignInAsync(user, isPersistent: false);
        await _userManager.AddToRoleAsync(user, "Customer");
        return true;
    }
    public async Task<bool> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return false;
        }
        if (!user.IsActive)
        {
            return false;
        }
        var result = await _signInManager.PasswordSignInAsync(
            user,
            dto.Password,
            dto.RememberMe,
            lockoutOnFailure: true
        );
        return result.Succeeded;
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

}
