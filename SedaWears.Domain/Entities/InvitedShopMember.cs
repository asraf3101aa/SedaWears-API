using SedaWears.Domain.Common;

namespace SedaWears.Domain.Entities;

public abstract class InvitedShopMember : BaseEntity
{
    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
