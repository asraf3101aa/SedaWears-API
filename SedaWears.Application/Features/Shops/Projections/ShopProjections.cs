using SedaWears.Application.Features.Shops.Models;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Shops.Projections;

public static class ShopProjections
{
    public static IQueryable<ShopDto> ProjectToShop(this IQueryable<Shop> query, string baseMediaUrl)
    {
        return query.Select(s => new ShopDto(
            s.Id,
            s.Name,
            s.SubdomainSlug,
            s.Description,
            string.IsNullOrEmpty(s.LogoFileName) ? null : baseMediaUrl + "/t/" + s.LogoFileName,
            string.IsNullOrEmpty(s.BannerFileName) ? null : baseMediaUrl + "/t/" + s.BannerFileName,
            s.IsActive,
            s.CreatedAt
        ));
    }
}
