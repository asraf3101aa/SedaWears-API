using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class InvitedAdminConfiguration : IEntityTypeConfiguration<InvitedAdmin>
{
    public void Configure(EntityTypeBuilder<InvitedAdmin> builder)
    {
        builder.ToTable("InvitedAdmins");
        builder.HasKey(ia => ia.Id);

        builder.Property(ia => ia.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ia => ia.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(ia => ia.Email)
            .IsUnique();

        builder.HasIndex(ia => ia.Token)
            .IsUnique();
    }
}
