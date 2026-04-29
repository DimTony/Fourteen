using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fourteen.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasConversion(id => id.Value, v => new UserId(v))
                .HasColumnName("id");

            builder.Property(p => p.GithubId)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("github_id");
            builder.HasIndex(p => p.GithubId).IsUnique();

            builder.Property(p => p.Username)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("username");
            builder.HasIndex(p => p.Username).IsUnique();

            builder.Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");
            builder.HasIndex(p => p.Email).IsUnique();

            builder.Property(p => p.AvatarUrl)
                .IsRequired()
                .HasColumnName("avatar_url");

            builder.Property(p => p.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasColumnName("role");

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            builder.Property(p => p.LastLoginAt)
                .HasColumnName("last_login_at");

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.HasIndex(p => p.CreatedAt);
            builder.HasIndex(p => p.IsActive);
        }
    }
}