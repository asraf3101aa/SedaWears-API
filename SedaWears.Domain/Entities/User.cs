using Microsoft.AspNetCore.Identity;

namespace SedaWears.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? AvatarFileName { get; set; }

    public ICollection<ShopOwner> ShopOwnerships { get; set; } = [];
    public ICollection<ShopManager> ShopManagements { get; set; } = [];
    public ICollection<Address> Addresses { get; set; } = [];
    public ICollection<WishlistItem> WishlistItems { get; set; } = [];
}
