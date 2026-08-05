using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Authentication
{
    public class RegisterDto
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
