using SedaWears.Application.Features.Users.Models;
using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Common.Validators;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Users.Queries;

public record GetShopManagersByShopIdQuery(
    int ShopId,
    int PageNumber = 1,
    int PageSize = 10,
    UsersSortBy SortBy = UsersSortBy.CreatedAt,
    SortOrder SortOrder = SortOrder.Desc)
    : IRequest<PaginatedList<UserDto>>, IPaginatedQuery;

public class GetShopManagersByShopIdValidator : PaginatedQueryValidator<GetShopManagersByShopIdQuery> { }

public class GetShopManagersByShopIdHandler(IUserService userService)
    : IRequestHandler<GetShopManagersByShopIdQuery, PaginatedList<UserDto>>
{
    public async Task<PaginatedList<UserDto>> Handle(GetShopManagersByShopIdQuery request, CancellationToken ct)
        => await userService.GetShopManagersAsync(
            request.ShopId,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortOrder, ct);
}
