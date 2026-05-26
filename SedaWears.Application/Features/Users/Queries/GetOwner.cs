using MediatR;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Users.Queries;

public record GetOwnerQuery(int Id) : IRequest<UserDto>;

public class GetOwnerHandler(IUserService userService) : IRequestHandler<GetOwnerQuery, UserDto>
{
    public async Task<UserDto> Handle(GetOwnerQuery request, CancellationToken ct)
        => await userService.GetUserByIdAndRoleAsync(request.Id, UserRole.Owner, ct);
}
