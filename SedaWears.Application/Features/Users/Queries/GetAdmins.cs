using SedaWears.Application.Features.Users.Models;
using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Common.Validators;

namespace SedaWears.Application.Features.Users.Queries;

public record GetAdminsQuery(
    int PageNumber,
    int PageSize,
    UsersSortBy SortBy,
    SortOrder SortOrder)
    : IRequest<PaginatedList<UserDto>>, IPaginatedQuery;

public class GetAdminsValidator : PaginatedQueryValidator<GetAdminsQuery> { }

public class GetAdminsHandler(IUserService userService) : IRequestHandler<GetAdminsQuery, PaginatedList<UserDto>>
{
    public async Task<PaginatedList<UserDto>> Handle(GetAdminsQuery request, CancellationToken ct)
        => await userService.GetUsersByRoleAsync(
            UserRole.Admin,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortOrder, ct);
}
