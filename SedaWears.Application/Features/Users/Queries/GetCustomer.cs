using SedaWears.Application.Features.Users.Models;
using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Users.Queries;

public record GetCustomerQuery(int Id) : IRequest<UserDto>;

public class GetCustomerHandler(IUserService userService) : IRequestHandler<GetCustomerQuery, UserDto>
{
    public async Task<UserDto> Handle(GetCustomerQuery request, CancellationToken ct)
        => await userService.GetUserByIdAndRoleAsync(request.Id, UserRole.Customer, ct);
}
