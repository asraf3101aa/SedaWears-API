using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Common.Interfaces;

public interface IUserService
{
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

    Task<UserDto> GetUserByIdAndRoleAsync(
        int userId,
        UserRole role,
        CancellationToken ct = default);
}
