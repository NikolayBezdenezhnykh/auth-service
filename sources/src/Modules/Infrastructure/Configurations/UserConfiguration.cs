using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace Infrastructure.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email).HasMaxLength(50);
            builder.Property(x => x.Password).HasMaxLength(256);

            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.UserId).IsUnique();

            builder.HasMany(x => x.Roles).WithMany(x => x.Users);
            builder.HasMany(x => x.UserVerifications).WithOne(x => x.User);
        }
    }
}
