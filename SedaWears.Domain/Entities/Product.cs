using SedaWears.Domain.Common;
using SedaWears.Domain.Enums;

namespace SedaWears.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Gender Gender { get; set; }
    public bool IsActive { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? DiscountId { get; set; }
    public Discount? Discount { get; set; }

    // Relationships
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<ProductSizeStock> SizeStocks { get; set; } = [];
}

