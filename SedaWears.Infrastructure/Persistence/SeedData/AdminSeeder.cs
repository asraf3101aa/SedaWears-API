using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Infrastructure.Persistence.Seeds;

public static class AdminSeeder
{
    public static async Task CreateAdminAsync(IServiceProvider serviceProvider, string email, string password, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));

        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Ensure roles are seeded first
        await RoleSeeder.SeedRolesAsync(serviceProvider);

        var existingAdmin = await userManager.FindByEmailAsync(email);
        if (existingAdmin != null && await userManager.IsInRoleAsync(existingAdmin, nameof(UserRole.Admin)))
        {
            return;
        }

        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,

        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, nameof(UserRole.Admin));
            Console.WriteLine($"Admin user {email} created successfully.");
        }
        else
        {
            Console.WriteLine("Failed to create admin user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
        }
    }
}
