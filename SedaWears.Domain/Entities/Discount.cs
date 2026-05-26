using SedaWears.Domain.Common;

namespace SedaWears.Domain.Entities;

public class Discount : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relationships
    public ICollection<Product> Products { get; set; } = [];
}
