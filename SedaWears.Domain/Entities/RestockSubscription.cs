using SedaWears.Domain.Common;
using SedaWears.Domain.Enums;

namespace SedaWears.Domain.Entities;

public class RestockSubscription : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public ProductSize Size { get; set; }
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public bool Notified { get; set; }
    public DateTime? NotifiedAt { get; set; }

    // Relationships
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Optional: null if guest, set if signed-up user
    public int? UserId { get; set; }
    public User? User { get; set; }
}
