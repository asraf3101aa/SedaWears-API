using System;
using System.Collections.Generic;
using SedaWears.Domain.Common;
using SedaWears.Domain.Enums;

namespace SedaWears.Domain.Entities;

#region Order
public class Order : BaseEntity
{
    public int? UserId { get; set; }
    public User? User { get; set; }

    public int? GuestId { get; set; }
    public Guest? Guest { get; set; }

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }

    public int? PromoCodeId { get; set; }
    public PromoCode? PromoCode { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];
}
#endregion

#region OrderItem
public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
#endregion

#region OrderStatus (Enum)
public enum OrderStatus
{
    Pending,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
#endregion

#region CartItem
public class CartItem : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? ShopId { get; set; }
    public Shop? Shop { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public ProductSize Size { get; set; }
    public int Quantity { get; set; }
}
#endregion

#region WishlistItem
public class WishlistItem : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
#endregion

#region PromoCode
public class PromoCode : BaseEntity
{
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public PromoCodeDiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? LimitPerUser { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? ShopId { get; set; }
    public Shop? Shop { get; set; }
}
#endregion
