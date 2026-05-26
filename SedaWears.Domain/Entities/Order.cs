using System;
using System.Collections.Generic;
using SedaWears.Domain.Common;

namespace SedaWears.Domain.Entities;

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

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public enum OrderStatus
{
    Pending,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
