using Microsoft.AspNetCore.Identity;
using MusicStore.Infrastructure.Identity;

namespace MusicStore.Infrastructure.seed;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        RoleManager<ApplicationRole> roleManager)
    {
        string[] roles =
        {
            "SuperAdmin",
            "Admin",
            "Customer"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role
                });
            }
        }
    }

    public static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@musicstore.com";

        var user = await userManager.FindByEmailAsync(adminEmail);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Bita",
                LastName = "Admin",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, "Admin12345");
        }

        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.RemoveFromRoleAsync(user, "Admin");
        }


        if (!await userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            await userManager.AddToRoleAsync(user, "SuperAdmin");
        }
    }
}