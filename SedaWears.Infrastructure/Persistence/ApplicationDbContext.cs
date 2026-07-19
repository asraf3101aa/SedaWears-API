using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, IdentityRole<int>, int>(options), IApplicationDbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<RestockSubscription> RestockSubscriptions => Set<RestockSubscription>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductSizeStock> ProductSizeStocks => Set<ProductSizeStock>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<ShopOwner> ShopOwners => Set<ShopOwner>();
    public DbSet<ShopManager> ShopManagers => Set<ShopManager>();
    public DbSet<InvitedAdmin> InvitedAdmins => Set<InvitedAdmin>();
    public DbSet<InvitedShopOwner> InvitedShopOwners => Set<InvitedShopOwner>();
    public DbSet<InvitedShopManager> InvitedShopManagers => Set<InvitedShopManager>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    DbSet<User> IApplicationDbContext.Users => Users;


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

}
