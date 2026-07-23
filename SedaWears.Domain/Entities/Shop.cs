using System;
using System.Collections.Generic;
using SedaWears.Domain.Common;
using SedaWears.Domain.Enums;

namespace SedaWears.Domain.Entities;

#region Shop
public class Shop : BaseEntity
{
    public string Name { get; set; } = null!;
    public string SubdomainSlug { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? BannerFileName { get; set; }
    public string? LogoFileName { get; set; }

    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ShopOwner> Owners { get; set; } = [];
    public ICollection<ShopManager> Managers { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
#endregion

#region ShopMember
public abstract class ShopMember : BaseEntity
{
    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
#endregion

#region ShopOwner
public class ShopOwner : ShopMember
{
}
#endregion

#region ShopManager
public class ShopManager : ShopMember
{
}
#endregion

#region InvitedShopMember
public abstract class InvitedShopMember : BaseEntity
{
    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
#endregion

#region InvitedShopOwner
public class InvitedShopOwner : InvitedShopMember
{
}
#endregion

#region InvitedShopManager
public class InvitedShopManager : InvitedShopMember
{
}
#endregion

#region Category
public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int DisplayOrder { get; set; }

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;
    public bool IsActive { get; set; } = false;

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
