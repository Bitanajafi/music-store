using MusicStore.Application.DTOs.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool>RegisterAsync(RegisterDto dto);

        Task<bool>LoginAsync(LoginDto dto);

        Task LogoutAsync();
    }
}
