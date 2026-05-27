using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Features.Products.Projections;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Common.Validators;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Products.Queries;

public record GetProductsQuery(
    int? CategoryId,
    int? ShopId,
    int PageNumber,
    int PageSize,
    ProductSortBy SortBy,
    SortOrder SortOrder,
    string? Search) : IRequest<PaginatedList<ProductDto>>, IPaginatedQuery;

public class GetProductsValidator : PaginatedQueryValidator<GetProductsQuery> { }

public class GetProductsHandler(IApplicationDbContext dbContext, OpeninaryConfig config) : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
    public async Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var query = dbContext.Products
            .AsNoTracking();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        query = query.Where(p => p.Category.ShopId == request.ShopId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.Trim();
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{searchTerm}%"));
        }

        var isDescending = request.SortOrder == SortOrder.Desc;
        query = request.SortBy switch
        {
            ProductSortBy.Name => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            ProductSortBy.Price => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            ProductSortBy.CreatedAt => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var products = await query.Skip((request.PageNumber - 1) * request.PageSize)
                                  .Take(request.PageSize)
                                  .ProjectToProduct(config.BaseUrl)
                                  .ToListAsync(ct);

        return new PaginatedList<ProductDto>(products, totalCount, request.PageNumber, request.PageSize);
    }
}
