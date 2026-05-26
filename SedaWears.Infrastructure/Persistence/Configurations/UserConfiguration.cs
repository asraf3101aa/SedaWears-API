using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedaWears.Domain.Entities;

namespace SedaWears.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // IdentityUser properties like Id, Email, UserName are already configured by base.OnModelCreating(builder)
        // using IdentityDbContext. We only need to configure our custom added properties.

        builder.Property(u => u.FirstName)
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        builder.Property(u => u.AvatarFileName)
            .HasMaxLength(512);



        // Indexing common filter properties

    }
}
