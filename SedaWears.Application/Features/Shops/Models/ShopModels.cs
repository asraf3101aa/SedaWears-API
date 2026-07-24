
namespace SedaWears.Application.Features.Shops.Models;

public record ShopDto(
    int Id,
    string Name,
    string SubdomainSlug,
    string Description,
    string? LogoFileName,
    Uri? LogoUrl,
    string? BannerFileName,
    Uri? BannerUrl,
    bool? IsActive,
    DateTime CreatedAt,
    bool IsDeleted
);