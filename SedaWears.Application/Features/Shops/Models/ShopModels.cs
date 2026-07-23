
namespace SedaWears.Application.Features.Shops.Models;

public record ShopDto(
    int Id,
    string Name,
    string SubdomainSlug,
    string Description,
    Uri? LogoUrl,
    Uri? BannerUrl,
    bool IsActive,
    DateTime CreatedAt
);