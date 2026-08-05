using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Users;

public class UserDto
{
    public string Id { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName => $"{FirstName} {LastName}";

    public string Email { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Role { get; set; } = null!;
}