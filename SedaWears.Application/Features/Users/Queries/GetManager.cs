using MediatR;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Users.Queries;

public record GetManagerQuery(int Id) : IRequest<UserDto>;

public class GetManagerHandler(IUserService userService) : IRequestHandler<GetManagerQuery, UserDto>
{
    public async Task<UserDto> Handle(GetManagerQuery request, CancellationToken ct)
        => await userService.GetUserByIdAndRoleAsync(request.Id, UserRole.Manager, ct);
}
