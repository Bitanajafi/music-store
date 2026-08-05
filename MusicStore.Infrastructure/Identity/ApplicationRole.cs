using Microsoft.AspNetCore.Identity;

namespace MusicStore.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
