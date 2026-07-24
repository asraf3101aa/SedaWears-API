using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.Description).HasMaxLength(5000);
        builder.Property(p => p.Gender).HasConversion<string>().HasMaxLength(32);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.Price);
        builder.HasIndex(p => p.Gender);
        builder.HasIndex(p => p.CategoryId);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
