using Microsoft.Extensions.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Shops.Models;
using SedaWears.Application.Features.Shops.Projections;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Common.Validators;
using SedaWears.Application.Common.Settings;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetMyShopsQuery(
    ShopSortBy SortBy,
    SortOrder SortOrder,
    string? Search,
    int PageNumber,
    int PageSize) : IRequest<PaginatedList<ShopDto>>, IPaginatedQuery;

public class GetMyShopsValidator : PaginatedQueryValidator<GetMyShopsQuery> { }

public class GetMyShopsHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IOptions<OpeninaryConfig> configOptions) : IRequestHandler<GetMyShopsQuery, PaginatedList<ShopDto>>
{
    public async Task<PaginatedList<ShopDto>> Handle(GetMyShopsQuery request, CancellationToken ct)
    {
        var currentUserId = currentUser!.Id;

        var query = dbContext.Shops
            .AsNoTracking()
            .Where(s => s.Owners.Any(o => o.UserId == currentUserId) || s.Managers.Any(m => m.UserId == currentUserId));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.Trim();
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{searchTerm}%") ||
                                     EF.Functions.ILike(s.SubdomainSlug, $"%{searchTerm}%"));
        }

        var isDescending = request.SortOrder == SortOrder.Desc;
        query = request.SortBy switch
        {
            ShopSortBy.Name => isDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            ShopSortBy.Slug => isDescending ? query.OrderByDescending(s => s.SubdomainSlug) : query.OrderBy(s => s.SubdomainSlug),
            ShopSortBy.IsActive => isDescending ? query.OrderByDescending(s => s.IsActive) : query.OrderBy(s => s.IsActive),
            ShopSortBy.CreatedAt => isDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => query.OrderByDescending(s => s.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToShop(configOptions.Value.BaseUrl)
            .ToListAsync(ct);

        return new PaginatedList<ShopDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
