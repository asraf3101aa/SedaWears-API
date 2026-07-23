using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Shops.Models;
using SedaWears.Application.Features.Shops.Projections;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Common.Validators;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetShopsQuery(
    ShopSortField SortBy,
    SortOrder SortOrder,
    string? Search,
    int PageNumber,
    int PageSize) : IRequest<PaginatedList<ShopDto>>, IPaginatedQuery;

public class GetShopsValidator : PaginatedQueryValidator<GetShopsQuery> { }

public class GetShopsHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext,
    IOptions<OpeninaryConfig> configOptions) : IRequestHandler<GetShopsQuery, PaginatedList<ShopDto>>
{
    public async Task<PaginatedList<ShopDto>> Handle(GetShopsQuery request, CancellationToken ct)
    {
        var query = dbContext.Shops.AsQueryable();

        if (originContext.OriginRole == UserRole.Customer)
        {
            query = query.Where(s => s.IsActive);
        }
        else
        {
            var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

            if (!user.Roles.Contains(originContext.OriginRole))
                throw new ForbiddenException("You are not authorized to view shops.");

            var userId = currentUser.Id;
            query = originContext.OriginRole switch
            {
                UserRole.Admin => query.Where(s => s.Id != 1),
                UserRole.Owner => query.Where(s => s.Id != 1 && s.Owners.Any(o => o.UserId == userId)),
                UserRole.Manager => query.Where(s => s.Id != 1 && s.Managers.Any(m => m.UserId == userId)),
                _ => throw new ForbiddenException("You are not authorized to view shops.")
            };
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.Trim();
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{searchTerm}%") ||
                                      EF.Functions.ILike(s.SubdomainSlug, $"%{searchTerm}%"));
        }

        var desc = request.SortOrder == SortOrder.Desc;
        query = request.SortBy switch
        {
            ShopSortField.Name => desc ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            ShopSortField.Slug => desc ? query.OrderByDescending(s => s.SubdomainSlug) : query.OrderBy(s => s.SubdomainSlug),
            ShopSortField.IsActive => desc ? query.OrderByDescending(s => s.IsActive) : query.OrderBy(s => s.IsActive),
            ShopSortField.CreatedAt => desc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => query.OrderByDescending(s => s.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .AsNoTracking()
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToShop(configOptions.Value.BaseUrl)
            .ToListAsync(ct);

        return new PaginatedList<ShopDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
