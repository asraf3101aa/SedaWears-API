using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SedaWears.Domain.Entities;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Application.Common.Services;

public class UserService(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    UserManager<User> userManager,
    IOptions<OpeninaryConfig> configOptions) : IUserService
{
    public async Task<PaginatedList<UserDto>> GetUsersByRoleAsync(
        UserRole role,
        int pageNumber,
        int pageSize,
        UsersSortBy sortBy = UsersSortBy.CreatedAt,
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
            UsersSortBy.Name => desc
                ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
            UsersSortBy.Email => desc
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            _ => desc
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .ProjectToUser(configOptions.Value.BaseUrl)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedList<UserDto>(items, total, pageNumber, pageSize);
    }

    public async Task<PaginatedList<UserDto>> GetShopManagersAsync(
        int shopId,
        int pageNumber,
        int pageSize,
        UsersSortBy sortBy = UsersSortBy.CreatedAt,
        SortOrder sortOrder = SortOrder.Desc,
        CancellationToken ct = default)
    {
        var query = dbContext.ShopManagers
            .AsNoTracking()
            .Where(sm => sm.ShopId == shopId && sm.UserId != currentUser.Id);

        var desc = sortOrder == SortOrder.Desc;
        query = sortBy switch
        {
            UsersSortBy.Name => desc
                ? query.OrderByDescending(sm => sm.User.FirstName).ThenByDescending(sm => sm.User.LastName)
                : query.OrderBy(sm => sm.User.FirstName).ThenBy(sm => sm.User.LastName),
            UsersSortBy.Email => desc
                ? query.OrderByDescending(sm => sm.User.Email)
                : query.OrderBy(sm => sm.User.Email),
            _ => desc
                ? query.OrderByDescending(sm => sm.User.CreatedAt)
                : query.OrderBy(sm => sm.User.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Select(sm => sm.User)
            .ProjectToUser(configOptions.Value.BaseUrl)
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
            .ProjectToUser(configOptions.Value.BaseUrl)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("User not found.");
    }
}
