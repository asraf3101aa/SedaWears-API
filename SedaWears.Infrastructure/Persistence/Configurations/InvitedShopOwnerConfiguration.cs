using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class InvitedShopOwnerConfiguration : IEntityTypeConfiguration<InvitedShopOwner>
{
    public void Configure(EntityTypeBuilder<InvitedShopOwner> builder)
    {
        builder.ToTable("InvitedShopOwners");
        builder.HasKey(iso => iso.Id);

        builder.Property(iso => iso.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(iso => iso.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(iso => new { iso.ShopId, iso.Email })
            .IsUnique();

        builder.HasIndex(iso => iso.Token)
            .IsUnique();

        builder.HasOne(iso => iso.Shop)
            .WithMany()
            .HasForeignKey(iso => iso.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
