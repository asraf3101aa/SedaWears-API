using SedaWears.Application.Features.Products.Models;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Products.Projections;

public static class ProductProjections
{
    public static IQueryable<ProductDto> ProjectToProduct(this IQueryable<Product> query, string baseMediaUrl)
    {
        return query.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Gender,
            p.Images.OrderBy(i => i.Order).Select(i => string.IsNullOrEmpty(i.FileName) ? "" : baseMediaUrl + "/t/" + i.FileName).ToList(),
            p.SizeStocks.Select(s => new ProductSizeDto(s.Size, s.Stock)).ToList(),
            new CategorySummary(p.Category.Id, p.Category.Name),
            p.CreatedAt
        ));
    }
}
