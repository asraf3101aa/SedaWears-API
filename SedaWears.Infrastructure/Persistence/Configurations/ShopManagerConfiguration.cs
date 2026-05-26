using SedaWears.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class ShopManagerConfiguration : IEntityTypeConfiguration<ShopManager>
{
    public void Configure(EntityTypeBuilder<ShopManager> builder)
    {
        builder.ToTable("ShopManagers");
        builder.HasKey(sm => sm.Id);

        builder.HasIndex(sm => new { sm.ShopId, sm.UserId })
            .IsUnique();

        builder.HasIndex(sm => sm.UserId);

        builder.HasOne(sm => sm.Shop)
            .WithMany(s => s.Managers)
            .HasForeignKey(sm => sm.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sm => sm.User)
            .WithMany(u => u.ShopManagements)
            .HasForeignKey(sm => sm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
