using SedaWears.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();


        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        // 1. Unique index for shop-specific categories
        builder.HasIndex(t => new { t.ShopId, t.Name })
            .HasFilter("\"ShopId\" IS NOT NULL")
            .IsUnique();


        // 2. Unique index for global categories (ShopId is null)
        builder.HasIndex(t => t.Name)
            .HasFilter("\"ShopId\" IS NULL")
            .IsUnique();

        
        builder.HasIndex(t => t.DisplayOrder);
        builder.HasIndex(t => t.ShopId);
        
        builder.HasOne(t => t.Shop)
            .WithMany()
            .HasForeignKey(t => t.ShopId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
