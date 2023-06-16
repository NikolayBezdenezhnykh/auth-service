using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace Infrastructure.Configurations
{
    internal sealed class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
    {
        public void Configure(EntityTypeBuilder<UserCredential> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Login).HasMaxLength(20);
            builder.Property(x => x.Password).HasMaxLength(256);

            builder.HasIndex(x => x.Login).IsUnique();
        }
    }
}
