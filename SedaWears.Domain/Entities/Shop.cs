using System;
using System.Collections.Generic;
using SedaWears.Domain.Common;

namespace SedaWears.Domain.Entities;

#region Shop
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

#region InvitedAdmin
public class InvitedAdmin : BaseEntity
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
#endregion
