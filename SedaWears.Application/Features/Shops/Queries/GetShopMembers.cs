using Microsoft.Extensions.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;
using SedaWears.Domain.Enums;
using SedaWears.Application.Common.Validators;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Shops.Models;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetShopMembersQuery(
    int ShopId,
    UserRole? Role,
    int PageNumber,
    int PageSize,
    ShopMemberSortBy SortBy,
    SortOrder SortOrder) : IRequest<PaginatedList<UserDto>>, IPaginatedQuery;

public class GetShopMembersValidator : PaginatedQueryValidator<GetShopMembersQuery> { }

public class GetShopMembersHandler(IApplicationDbContext dbContext, IOptions<OpeninaryConfig> configOptions)
    : IRequestHandler<GetShopMembersQuery, PaginatedList<UserDto>>
{
    public async Task<PaginatedList<UserDto>> Handle(GetShopMembersQuery request, CancellationToken ct)
    {
        var role = request.Role ?? UserRole.Manager;
        var desc = request.SortOrder == SortOrder.Desc;

        if (role == UserRole.Owner)
        {
            var query = dbContext.ShopOwners
                .AsNoTracking()
                .Where(so => so.ShopId == request.ShopId);

            query = request.SortBy switch
            {
                ShopMemberSortBy.Name => desc
                    ? query.OrderByDescending(so => so.User.FirstName).ThenByDescending(so => so.User.LastName)
                    : query.OrderBy(so => so.User.FirstName).ThenBy(so => so.User.LastName),
                ShopMemberSortBy.Email => desc
                    ? query.OrderByDescending(so => so.User.Email)
                    : query.OrderBy(so => so.User.Email),
                ShopMemberSortBy.CreatedAt => desc
                    ? query.OrderByDescending(so => so.CreatedAt)
                    : query.OrderBy(so => so.CreatedAt),
                _ => query.OrderByDescending(so => so.CreatedAt)
            };

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(so => so.User)
                .ProjectToUser(configOptions.Value.BaseUrl)
                .ToListAsync(ct);

            return new PaginatedList<UserDto>(items, total, request.PageNumber, request.PageSize);
        }
        else
        {
            var query = dbContext.ShopManagers
                .AsNoTracking()
                .Where(sm => sm.ShopId == request.ShopId);

            query = request.SortBy switch
            {
                ShopMemberSortBy.Name => desc
                    ? query.OrderByDescending(sm => sm.User.FirstName).ThenByDescending(sm => sm.User.LastName)
                    : query.OrderBy(sm => sm.User.FirstName).ThenBy(sm => sm.User.LastName),
                ShopMemberSortBy.Email => desc
                    ? query.OrderByDescending(sm => sm.User.Email)
                    : query.OrderBy(sm => sm.User.Email),
                ShopMemberSortBy.CreatedAt => desc
                    ? query.OrderByDescending(sm => sm.User.CreatedAt)
                    : query.OrderBy(sm => sm.User.CreatedAt),
                _ => query.OrderByDescending(sm => sm.User.CreatedAt)
            };

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(sm => sm.User)
                .ProjectToUser(configOptions.Value.BaseUrl)
                .ToListAsync(ct);

            return new PaginatedList<UserDto>(items, total, request.PageNumber, request.PageSize);
        }
    }
}
