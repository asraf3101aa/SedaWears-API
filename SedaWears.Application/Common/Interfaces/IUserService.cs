
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Users.Models;

namespace SedaWears.Application.Common.Interfaces;

public interface IUserService
{
    Task<UserDto?> FindByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<User?> FindByEmailAsync(string email);

    Task<bool> IsInRoleAsync(User user, string role);
    Task<List<User>> GetUsersInRoleAsync(string role);
    
    Task CreateUserAsync(User user, string password);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(User user);
    
    Task AddToRoleAsync(User user, string role);
    Task RemoveFromRoleAsync(User user, string role);
    
    Task ChangePasswordAsync(User user, string oldPassword, string newPassword);
    
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task ResetPasswordAsync(User user, string token, string newPassword);
    Task ConfirmEmailAsync(User user, string token);

    Task<PaginatedList<UserDto>> GetUsersByRoleAsync(
        UserRole role,
        int pageNumber,
        int pageSize,
        UsersSortBy sortBy = UsersSortBy.CreatedAt,
        SortOrder sortOrder = SortOrder.Desc,
        CancellationToken ct = default);

    Task<PaginatedList<UserDto>> GetShopManagersAsync(
        int shopId,
        int pageNumber,
        int pageSize,
        UsersSortBy sortBy = UsersSortBy.CreatedAt,
        SortOrder sortOrder = SortOrder.Desc,
        CancellationToken ct = default);

    Task<UserDto> GetUserByIdAndRoleAsync(int userId, UserRole role, CancellationToken ct = default);

    Task SyncMemberRoleAsync(int userId, CancellationToken ct = default);
}
