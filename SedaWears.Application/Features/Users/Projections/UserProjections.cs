using SedaWears.Application.Features.Users.Models;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Users.Projections;

public static class UserProjections
{
    public static IQueryable<UserDto> ProjectToUser(this IQueryable<User> query, string baseMediaUrl)
    {
        return query.Select(u => new UserDto(
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.PhoneNumber,
            string.IsNullOrEmpty(u.AvatarFileName) ? null : baseMediaUrl + "/t/" + u.AvatarFileName,
            u.EmailConfirmed,
            u.CreatedAt
        ));
    }
}
