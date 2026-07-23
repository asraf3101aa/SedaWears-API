using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Application.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SedaWears.Domain.Enums;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;
using SedaWears.Application.Common.Settings;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Infrastructure.Services;

public class UserService(
    UserManager<User> userManager,
    IApplicationDbContext dbContext,
    IFusionCache fusionCache,
    IServiceScopeFactory scopeFactory,
    ICurrentUser currentUser,
    IOptions<OpeninaryConfig> configOptions) : IUserService
{
    public async Task<UserDto?> FindByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await fusionCache.GetOrSetAsync(
            CacheKeys.User(userId),
            async (ct) =>
            {
                using var scope = scopeFactory.CreateScope();
                var scopedDb = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                return await scopedDb.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .ProjectToUser(configOptions.Value.BaseUrl, scopedDb)
                    .FirstOrDefaultAsync(ct);
            },
            CachePolicies.UserProfile,
            token: cancellationToken);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await userManager.FindByEmailAsync(email);
    }

    public async Task<bool> IsInRoleAsync(User user, string role)
    {
        return await userManager.IsInRoleAsync(user, role);
    }

    public async Task<List<User>> GetUsersInRoleAsync(string role)
    {
        var users = await userManager.GetUsersInRoleAsync(role);
        return [.. users];
    }

    public async Task CreateUserAsync(User user, string password)
    {
        var result = await userManager.CreateAsync(user, password);
        result.ThrowIfFailed();
    }

    public async Task UpdateUserAsync(User user)
    {
        var result = await userManager.UpdateAsync(user);
        result.ThrowIfFailed();
    }

    public async Task DeleteUserAsync(User user)
    {
        var result = await userManager.DeleteAsync(user);
        result.ThrowIfFailed();
    }

    public async Task AddToRoleAsync(User user, string role)
    {
        var result = await userManager.AddToRoleAsync(user, role);
        result.ThrowIfFailed();
    }

    public async Task RemoveFromRoleAsync(User user, string role)
    {
        var result = await userManager.RemoveFromRoleAsync(user, role);
        result.ThrowIfFailed();
    }

    public async Task ChangePasswordAsync(User user, string oldPassword, string newPassword)
    {
        var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        result.ThrowIfFailed();
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task ResetPasswordAsync(User user, string token, string newPassword)
    {
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        result.ThrowIfFailed();
    }

    public async Task ConfirmEmailAsync(User user, string token)
    {
        var result = await userManager.ConfirmEmailAsync(user, token);
        result.ThrowIfFailed();
    }

    public async Task<PaginatedList<UserDto>> GetUsersByRoleAsync(
        UserRole role,
        int pageNumber,
        int pageSize,
        UsersSortField sortBy = UsersSortField.CreatedAt,
        SortOrder sortOrder = SortOrder.Desc,
        CancellationToken ct = default)
    {
        var usersInRole = await userManager.GetUsersInRoleAsync(role.ToString());
        var userIds = usersInRole.Select(u => u.Id).ToList();

        var query = userManager.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id) && u.Id != currentUser.Id);

        var desc = sortOrder == SortOrder.Desc;
        query = sortBy switch
        {
            UsersSortField.Name => desc
                ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
            UsersSortField.Email => desc
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            _ => desc
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .ProjectToUser(configOptions.Value.BaseUrl, dbContext)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedList<UserDto>(items, total, pageNumber, pageSize);
    }

    public async Task<PaginatedList<UserDto>> GetShopManagersAsync(
        int shopId,
        int pageNumber,
        int pageSize,
        UsersSortField sortBy = UsersSortField.CreatedAt,
        SortOrder sortOrder = SortOrder.Desc,
        CancellationToken ct = default)
    {
        var query = dbContext.ShopManagers
            .AsNoTracking()
            .Where(sm => sm.ShopId == shopId && sm.UserId != currentUser.Id);

        var desc = sortOrder == SortOrder.Desc;
        query = sortBy switch
        {
            UsersSortField.Name => desc
                ? query.OrderByDescending(sm => sm.User.FirstName).ThenByDescending(sm => sm.User.LastName)
                : query.OrderBy(sm => sm.User.FirstName).ThenBy(sm => sm.User.LastName),
            UsersSortField.Email => desc
                ? query.OrderByDescending(sm => sm.User.Email)
                : query.OrderBy(sm => sm.User.Email),
            _ => desc
                ? query.OrderByDescending(sm => sm.User.CreatedAt)
                : query.OrderBy(sm => sm.User.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Select(sm => sm.User)
            .ProjectToUser(configOptions.Value.BaseUrl, dbContext)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedList<UserDto>(items, total, pageNumber, pageSize);
    }

    public async Task<UserDto> GetUserByIdAndRoleAsync(int userId, UserRole role, CancellationToken ct = default)
    {
        var usersInRole = await userManager.GetUsersInRoleAsync(role.ToString());
        if (!usersInRole.Any(u => u.Id == userId))
            throw new NotFoundException("User not found.");

        return await userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .ProjectToUser(configOptions.Value.BaseUrl, dbContext)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("User not found.");
    }

    public async Task SyncMemberRoleAsync(int userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return;

        var hasOwnerMembership = await dbContext.ShopOwners.AnyAsync(so => so.UserId == userId, ct);
        var hasManagerMembership = await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == userId, ct);

        if (!hasOwnerMembership && await userManager.IsInRoleAsync(user, UserRole.Owner.ToString()))
            await userManager.RemoveFromRoleAsync(user, UserRole.Owner.ToString());

        if (!hasManagerMembership && await userManager.IsInRoleAsync(user, UserRole.Manager.ToString()))
            await userManager.RemoveFromRoleAsync(user, UserRole.Manager.ToString());

        var remainingRoles = await userManager.GetRolesAsync(user);
        if (remainingRoles.Count == 0)
            await userManager.DeleteAsync(user);
    }
}

public static class IdentityResultExtensions
{
    public static void ThrowIfFailed(this IdentityResult result)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }
    }
}
