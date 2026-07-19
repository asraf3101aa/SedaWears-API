using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Users.Projections;

public static class UserProjections
{
    public static IQueryable<UserDto> ProjectToUser(this IQueryable<User> query, string baseMediaUrl, IApplicationDbContext dbContext)
    {
        return query.Select(u => new UserDto(
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.PhoneNumber,
            string.IsNullOrEmpty(u.AvatarFileName) ? null : new Uri(baseMediaUrl + "/" + u.AvatarFileName),
            u.EmailConfirmed,
            u.CreatedAt,
            dbContext.UserRoles
                .Where(ur => ur.UserId == u.Id)
                .Join(
                    dbContext.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => Enum.Parse<UserRole>(r.Name!)
                ).ToList()
        ));
    }
}
