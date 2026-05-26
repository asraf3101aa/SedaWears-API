using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;
using SedaWears.Domain.Enums;
using SedaWears.Application.Common.Validators;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetShopMembersQuery(
    int ShopId,
    UserRole? Role = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = "createdAt",
    string? SortOrder = "desc") : IRequest<PaginatedList<UserDto>>, IPaginatedQuery;

public class GetShopMembersValidator : PaginatedQueryValidator<GetShopMembersQuery> { }

public class GetShopMembersHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetShopMembersQuery, PaginatedList<UserDto>>
{
    public async Task<PaginatedList<UserDto>> Handle(GetShopMembersQuery request, CancellationToken ct)
    {
        var role = request.Role ?? UserRole.Manager;
        var desc = request.SortOrder?.ToLower() == "desc";

        if (role == UserRole.Owner)
        {
            var query = dbContext.ShopOwners
                .AsNoTracking()
                .Where(so => so.ShopId == request.ShopId);

            query = request.SortBy?.ToLower() switch
            {
                "name" => desc
                    ? query.OrderByDescending(so => so.User.FirstName).ThenByDescending(so => so.User.LastName)
                    : query.OrderBy(so => so.User.FirstName).ThenBy(so => so.User.LastName),
                "email" => desc
                    ? query.OrderByDescending(so => so.User.Email)
                    : query.OrderBy(so => so.User.Email),
                _ => desc
                    ? query.OrderByDescending(so => so.CreatedAt)
                    : query.OrderBy(so => so.CreatedAt)
            };

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(so => so.User)
                .ProjectToUser()
                .ToListAsync(ct);

            return new PaginatedList<UserDto>(items, total, request.PageNumber, request.PageSize);
        }
        else
        {
            var query = dbContext.ShopManagers
                .AsNoTracking()
                .Where(sm => sm.ShopId == request.ShopId);

            query = request.SortBy?.ToLower() switch
            {
                "name" => desc
                    ? query.OrderByDescending(sm => sm.User.FirstName).ThenByDescending(sm => sm.User.LastName)
                    : query.OrderBy(sm => sm.User.FirstName).ThenBy(sm => sm.User.LastName),
                "email" => desc
                    ? query.OrderByDescending(sm => sm.User.Email)
                    : query.OrderBy(sm => sm.User.Email),
                _ => desc
                    ? query.OrderByDescending(sm => sm.CreatedAt)
                    : query.OrderBy(sm => sm.CreatedAt)
            };

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(sm => sm.User)
                .ProjectToUser()
                .ToListAsync(ct);

            return new PaginatedList<UserDto>(items, total, request.PageNumber, request.PageSize);
        }
    }
}
