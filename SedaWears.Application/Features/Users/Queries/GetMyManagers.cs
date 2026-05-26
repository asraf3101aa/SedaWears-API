using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Common.Validators;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Users.Queries;

public record GetMyManagersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    UsersSortBy SortBy = UsersSortBy.CreatedAt,
    SortOrder SortOrder = SortOrder.Desc)
    : IRequest<PaginatedList<UserDto>>, IPaginatedQuery;

public class GetMyManagersValidator : PaginatedQueryValidator<GetMyManagersQuery> { }

public class GetMyManagersHandler(IUserService userService, ICurrentUser currentUser)
    : IRequestHandler<GetMyManagersQuery, PaginatedList<UserDto>>
{
    public async Task<PaginatedList<UserDto>> Handle(GetMyManagersQuery request, CancellationToken ct)
    {
        var shopId = currentUser.ShopId ?? throw new UnauthorizedAccessException("Shop context missing. Please provide X-Shop-ID header.");

        return await userService.GetShopManagersAsync(
            shopId,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortOrder, ct);
    }
}
