using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Categories.Models;
using SedaWears.Application.Features.Categories.Projections;
using SedaWears.Application.Common.Validators;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Categories.Queries;

public record GetCategoriesQuery(
    int? ShopId,
    int PageNumber,
    int PageSize,
    CategorySortBy SortBy,
    SortOrder SortOrder,
    string? Search) : IRequest<PaginatedList<CategoryDto>>, IPaginatedQuery;

public class GetCategoriesValidator : PaginatedQueryValidator<GetCategoriesQuery> { }

public class GetCategoriesHandler(IApplicationDbContext dbContext) : IRequestHandler<GetCategoriesQuery, PaginatedList<CategoryDto>>
{
    public async Task<PaginatedList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var query = dbContext.Categories
            .AsNoTracking();

        if (request.ShopId.HasValue)
            query = query.Where(c => c.ShopId == request.ShopId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.Trim();
            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{searchTerm}%"));
        }

        var isDescending = request.SortOrder == SortOrder.Desc;
        query = request.SortBy switch
        {
            CategorySortBy.Name => isDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            CategorySortBy.IsActive => isDescending ? query.OrderByDescending(c => c.IsActive) : query.OrderBy(c => c.IsActive),
            CategorySortBy.DisplayOrder => isDescending ? query.OrderByDescending(c => c.DisplayOrder) : query.OrderBy(c => c.DisplayOrder),
            _ => query.OrderByDescending(c => c.DisplayOrder)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToCategory()
            .ToListAsync(ct);

        return new PaginatedList<CategoryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
