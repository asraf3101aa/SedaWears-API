using SedaWears.Domain.Common;
using SedaWears.Domain.Enums;

namespace SedaWears.Domain.Entities;

#region Category
public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }

    public int? ShopId { get; set; }
    public Shop? Shop { get; set; }
    public bool IsActive { get; set; } = false;

    // Relationships
    public ICollection<Product> Products { get; set; } = [];
}
#endregion

#region Product
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
#endregion


#region ProductImage
public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public int Order { get; set; }
}
#endregion

#region ProductSizeStock
public class ProductSizeStock : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public ProductSize Size { get; set; }
    public int Stock { get; set; }
}
#endregion

#region Discount
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
#endregion
