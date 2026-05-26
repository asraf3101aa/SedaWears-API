using SedaWears.Application.Features.Users.Models;
using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Users.Queries;

public record GetAdminQuery(int Id) : IRequest<UserDto>;

public class GetAdminHandler(IUserService userService) : IRequestHandler<GetAdminQuery, UserDto>
{
    public async Task<UserDto> Handle(GetAdminQuery request, CancellationToken ct)
        => await userService.GetUserByIdAndRoleAsync(request.Id, UserRole.Admin, ct);
}
