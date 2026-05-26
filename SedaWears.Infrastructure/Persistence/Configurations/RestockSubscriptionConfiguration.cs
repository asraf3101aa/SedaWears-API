using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class RestockSubscriptionConfiguration : IEntityTypeConfiguration<RestockSubscription>
{
    public void Configure(EntityTypeBuilder<RestockSubscription> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Email).IsRequired().HasMaxLength(256);
        builder.Property(r => r.Size)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.ProductId);
        // Prevent duplicate subscriptions for same email + product + size
        builder.HasIndex(r => new { r.Email, r.ProductId, r.Size }).IsUnique();
    }
}
