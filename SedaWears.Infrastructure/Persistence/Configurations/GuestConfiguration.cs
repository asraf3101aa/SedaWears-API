using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(g => g.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Phone)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(g => g.Street)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(g => g.City)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(g => g.ZipCode)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasIndex(g => g.Email);
    }
}
