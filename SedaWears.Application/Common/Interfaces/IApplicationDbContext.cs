using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Identity;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<User> Users { get; }
    DbSet<IdentityRole<int>> Roles { get; }
    DbSet<IdentityUserRole<int>> UserRoles { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<NewsletterSubscriber> NewsletterSubscribers { get; }
    DbSet<Address> Addresses { get; }
    DbSet<RestockSubscription> RestockSubscriptions { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductSizeStock> ProductSizeStocks { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Shop> Shops { get; }
    DbSet<ShopOwner> ShopOwners { get; }
    DbSet<ShopManager> ShopManagers { get; }
    DbSet<InvitedAdmin> InvitedAdmins { get; }
    DbSet<InvitedShopOwner> InvitedShopOwners { get; }
    DbSet<InvitedShopManager> InvitedShopManagers { get; }
    DbSet<WishlistItem> WishlistItems { get; }
    DbSet<Guest> Guests { get; }
    DbSet<PromoCode> PromoCodes { get; }

    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

}
