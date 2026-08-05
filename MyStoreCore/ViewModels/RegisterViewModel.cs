using System.ComponentModel.DataAnnotations;

namespace MusicStore.Web.ViewModels.Account;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "نام")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;


    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن یکسان نیستند.")]
    [Display(Name = "تکرار رمز عبور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
