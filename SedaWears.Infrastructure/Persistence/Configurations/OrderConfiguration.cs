using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Shop)
            .WithMany()
            .HasForeignKey(o => o.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Guest)
            .WithMany()
            .HasForeignKey(o => o.GuestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(o => o.DiscountAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasOne(o => o.PromoCode)
            .WithMany()
            .HasForeignKey(o => o.PromoCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.GuestId);
        builder.HasIndex(o => o.ShopId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PromoCodeId);
    }
}
