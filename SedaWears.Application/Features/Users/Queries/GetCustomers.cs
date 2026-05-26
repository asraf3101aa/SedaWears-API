using SedaWears.Application.Features.Users.Models;
using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Common.Validators;

namespace SedaWears.Application.Features.Users.Queries;

public record GetCustomersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    UsersSortBy SortBy = UsersSortBy.CreatedAt,
    SortOrder SortOrder = SortOrder.Desc)
    : IRequest<PaginatedList<UserDto>>, IPaginatedQuery;

public class GetCustomersValidator : PaginatedQueryValidator<GetCustomersQuery> { }

public class GetCustomersHandler(IUserService userService) : IRequestHandler<GetCustomersQuery, PaginatedList<UserDto>>
{
    public async Task<PaginatedList<UserDto>> Handle(GetCustomersQuery request, CancellationToken ct)
        => await userService.GetUsersByRoleAsync(
            UserRole.Customer,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortOrder, ct);
}
