using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SedaWears.Domain.Enums;

namespace SedaWears.Infrastructure.Persistence.Seeds;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        foreach (var roleName in Enum.GetNames<UserRole>())
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                if (result.Succeeded)
                {
                    Console.WriteLine($"Role {roleName} created successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to create role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"Role {roleName} already exists.");
            }
        }
    }
}
