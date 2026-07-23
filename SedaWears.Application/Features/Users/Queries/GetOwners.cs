using SedaWears.Application.Features.Users.Models;
using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Common.Validators;

namespace SedaWears.Application.Features.Users.Queries;

public record GetOwnersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    UsersSortField SortBy = UsersSortField.CreatedAt,
    SortOrder SortOrder = SortOrder.Desc)
    : IRequest<PaginatedList<UserDto>>, IPaginatedQuery;

public class GetOwnersValidator : PaginatedQueryValidator<GetOwnersQuery> { }

public class GetOwnersHandler(IUserService userService) : IRequestHandler<GetOwnersQuery, PaginatedList<UserDto>>
{
    public async Task<PaginatedList<UserDto>> Handle(GetOwnersQuery request, CancellationToken ct)
        => await userService.GetUsersByRoleAsync(
            UserRole.Owner,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortOrder, ct);
}
