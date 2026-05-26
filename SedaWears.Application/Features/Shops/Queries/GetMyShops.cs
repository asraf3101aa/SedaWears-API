using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Shops.Models;
using SedaWears.Application.Features.Shops.Projections;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Common.Validators;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetMyShopsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = "createdAt",
    string? SortOrder = "desc",
    string? Search = null) : IRequest<PaginatedList<ShopDto>>, IPaginatedQuery;

public class GetMyShopsValidator : PaginatedQueryValidator<GetMyShopsQuery> { }

public class GetMyShopsHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<GetMyShopsQuery, PaginatedList<ShopDto>>
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

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            var isDescending = request.SortOrder?.ToLower() == "desc";
            query = request.SortBy.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                "slug" => isDescending ? query.OrderByDescending(s => s.SubdomainSlug) : query.OrderBy(s => s.SubdomainSlug),
                "isactive" => isDescending ? query.OrderByDescending(s => s.IsActive) : query.OrderBy(s => s.IsActive),
                "createdat" => isDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
                _ => isDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(s => s.CreatedAt);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToShop()
            .ToListAsync(ct);

        return new PaginatedList<ShopDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
