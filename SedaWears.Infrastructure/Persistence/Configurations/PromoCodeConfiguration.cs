using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.DiscountType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.DiscountValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.MinimumOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.MaxDiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Relation to Shop
        builder.HasOne(p => p.Shop)
            .WithMany()
            .HasForeignKey(p => p.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique indexes
        // 1. Enforce Code is unique globally for global promo codes (where ShopId IS NULL)
        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasFilter("\"ShopId\" IS NULL");

        // 2. Enforce Code is unique per shop for shop-level promo codes (where ShopId IS NOT NULL)
        builder.HasIndex(p => new { p.Code, p.ShopId })
            .IsUnique()
            .HasFilter("\"ShopId\" IS NOT NULL");

        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.StartDate);
        builder.HasIndex(p => p.EndDate);
    }
}
