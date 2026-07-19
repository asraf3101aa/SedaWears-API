using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using SedaWears.Domain.Common;
using SedaWears.Domain.Enums;

namespace SedaWears.Domain.Entities;

#region User
public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? AvatarFileName { get; set; }

    public ICollection<ShopOwner> ShopOwnerships { get; set; } = [];
    public ICollection<ShopManager> ShopManagements { get; set; } = [];
    public ICollection<Address> Addresses { get; set; } = [];
    public ICollection<WishlistItem> WishlistItems { get; set; } = [];
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

#region Address
public class Address : BaseEntity
{
    public string Label { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

    // Relationship
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
#endregion

#region Guest
public class Guest : BaseEntity
{
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
#endregion

#region NewsletterSubscriber
public class NewsletterSubscriber : BaseEntity
{
    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public string Email { get; set; } = string.Empty;
    public bool IsSubscribed { get; set; } = true;
    public string UnsubscribeToken { get; set; } = string.Empty;
}
#endregion

#region RestockSubscription
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
#endregion
