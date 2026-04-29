using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fourteen.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasConversion(id => id.Value, v => new RefreshTokenId(v))
                .HasColumnName("id");

            builder.Property(p => p.UserId)
                .IsRequired()
                .HasColumnName("user_id");

            builder.Property(p => p.Token)
                .IsRequired()
                .HasColumnName("token");

            builder.Property(p => p.IsRevoked)
                .IsRequired()
                .HasColumnName("is_revoked");

            builder.Property(p => p.ExpiresAt)
                .IsRequired()
                .HasColumnName("expires_at");

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.HasIndex(p => p.UserId);
            builder.HasIndex(p => p.CreatedAt);
            builder.HasIndex(p => p.ExpiresAt);
            builder.HasIndex(p => p.IsRevoked);
        }
    }
}
