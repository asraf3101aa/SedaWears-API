using SedaWears.Application.Features.Categories.Models;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Categories.Projections;

public static class CategoryProjections
{
    public static IQueryable<CategoryDto> ProjectToCategory(this IQueryable<Category> query)
    {
        return query.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.DisplayOrder,
            c.IsActive,
            c.IsDeleted
        ));
    }
}
