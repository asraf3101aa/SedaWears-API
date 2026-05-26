using SedaWears.Domain.Common;

namespace SedaWears.Domain.Entities;

public class Shop : BaseEntity
{
    public string Name { get; set; } = null!;
    public string SubdomainSlug { get; set; } = null!;
    public string? Description { get; set; }
    public string? BannerFileName { get; set; }
    public string? LogoFileName { get; set; }

    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relationships
    public ICollection<ShopOwner> Owners { get; set; } = [];
    public ICollection<ShopManager> Managers { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
