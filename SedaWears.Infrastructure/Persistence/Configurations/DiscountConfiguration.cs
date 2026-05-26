using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.DiscountPercentage)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(d => d.StartDate)
            .IsRequired();

        builder.Property(d => d.EndDate)
            .IsRequired();

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.HasIndex(d => d.Name);
        builder.HasIndex(d => d.IsActive);
        builder.HasIndex(d => d.StartDate);
        builder.HasIndex(d => d.EndDate);
    }
}
