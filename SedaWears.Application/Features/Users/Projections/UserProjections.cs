using SedaWears.Application.Features.Users.Models;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Users.Projections;

public static class UserProjections
{
    public static IQueryable<UserDto> ProjectToUser(this IQueryable<User> query)
    {
        return query.Select(u => new UserDto(
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.PhoneNumber,
            u.AvatarFileName,
            u.EmailConfirmed,
            u.CreatedAt
        ));
    }
}
