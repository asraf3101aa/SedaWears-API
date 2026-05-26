using SedaWears.Domain.Common;

namespace SedaWears.Domain.Entities;

public abstract class ShopMember : BaseEntity
{
    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
