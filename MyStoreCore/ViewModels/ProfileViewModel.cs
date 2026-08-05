using System.ComponentModel.DataAnnotations;

namespace MyStoreCore.ViewModels
{
    public class ProfileViewModel
    {

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        [MinLength(8, ErrorMessage = "رمز باید حداقل 8 کاراکتر باشد")]
        public string? NewPassword { get; set; }


        [Compare("NewPassword", ErrorMessage = "رمزها یکسان نیستند")]
        public string? ConfirmPassword { get; set; }


        public string? CurrentPassword { get; set; }
    }
}

