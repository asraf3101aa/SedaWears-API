using MediatR;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Users.Models;

namespace SedaWears.Application.Features.Profile.Queries;

public record GetMeQuery : IRequest<UserDto>;

public class GetMeHandler(
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext) : IRequestHandler<GetMeQuery, UserDto>
{
    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct);

        if (user is null || !user.Roles.Contains(originContext.OriginRole))
            throw new UserNotFoundException();

        return user;
    }
}
