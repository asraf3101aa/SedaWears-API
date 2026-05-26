using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class InvitedShopManagerConfiguration : IEntityTypeConfiguration<InvitedShopManager>
{
    public void Configure(EntityTypeBuilder<InvitedShopManager> builder)
    {
        builder.ToTable("InvitedShopManagers");
        builder.HasKey(ism => ism.Id);

        builder.Property(ism => ism.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ism => ism.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(ism => new { ism.ShopId, ism.Email })
            .IsUnique();

        builder.HasIndex(ism => ism.Token)
            .IsUnique();

        builder.HasOne(ism => ism.Shop)
            .WithMany()
            .HasForeignKey(ism => ism.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
