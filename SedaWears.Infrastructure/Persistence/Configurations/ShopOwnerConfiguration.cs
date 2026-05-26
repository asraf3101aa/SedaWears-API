using SedaWears.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class ShopOwnerConfiguration : IEntityTypeConfiguration<ShopOwner>
{
    public void Configure(EntityTypeBuilder<ShopOwner> builder)
    {
        builder.ToTable("ShopOwners");
        builder.HasKey(so => so.Id);

        builder.HasIndex(so => new { so.ShopId, so.UserId })
            .IsUnique();

        builder.HasIndex(so => so.UserId);

        builder.HasOne(so => so.Shop)
            .WithMany(s => s.Owners)
            .HasForeignKey(so => so.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(so => so.User)
            .WithMany(u => u.ShopOwnerships)
            .HasForeignKey(so => so.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
