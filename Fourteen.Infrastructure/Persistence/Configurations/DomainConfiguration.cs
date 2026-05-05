using Fourteen.Domain.Aggregates.Domains;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fourteen.Infrastructure.Persistence.Configurations
{
    public class DomainConfiguration : IEntityTypeConfiguration<Fourteen.Domain.Aggregates.Domains.Domain>
    {
        public void Configure(EntityTypeBuilder<Fourteen.Domain.Aggregates.Domains.Domain> builder)
        {
            builder.ToTable("domains");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasConversion(id => id.Value, v => new DomainId(v))
                .HasColumnName("id");

            builder.Property(p => p.OwnerId)
                .HasConversion(id => id.Value, v => new UserId(v))
                .HasColumnName("user_id");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");

            builder.Property(p => p.VerificationStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasColumnName("verification_status");


            builder.Property(p => p.VerificationToken)
                .IsRequired()
                .HasColumnName("verification_token");

            builder.Property(p => p.VerifiedAt)
                .HasColumnName("verified_at");

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.HasIndex(p => p.CreatedAt);
            builder.HasIndex(p => p.VerificationStatus);
        }
    }
}