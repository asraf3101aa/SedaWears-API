using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Label).IsRequired().HasMaxLength(64);
        builder.Property(a => a.FullName).IsRequired().HasMaxLength(256);
        builder.Property(a => a.Email).IsRequired().HasMaxLength(256);
        builder.Property(a => a.Phone).IsRequired().HasMaxLength(32);
        builder.Property(a => a.Street).IsRequired().HasMaxLength(512);
        builder.Property(a => a.City).IsRequired().HasMaxLength(128);
        builder.Property(a => a.ZipCode).IsRequired().HasMaxLength(32);

        builder.HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId);
    }
}
