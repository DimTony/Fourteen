using Fourteen.Domain.Aggregates.Domains;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fourteen.Infrastructure.Persistence.Configurations
{
    public class ScanConfiguration : IEntityTypeConfiguration<Scan>
    {
        public void Configure(EntityTypeBuilder<Scan> builder)
        {
            builder.ToTable("scans");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasConversion(id => id.Value, v => new ScanId(v))
                .HasColumnName("id");

            builder.Property(s => s.DomainId)
            .HasConversion(id => id.Value, v => new DomainId(v))
            .HasColumnName("domain_id");

            builder.Property(s => s.RequestedBy)
                .HasConversion(id => id.Value, v => new UserId(v))
                .HasColumnName("requested_by");

            builder.Property(s => s.Status)
                .HasConversion<string>()
                .HasColumnName("status");

            builder.Property(s => s.Type)
                .HasConversion<string>()
                .HasColumnName("type");

            builder.Property(p => p.StartedAt)
                .HasColumnName("started_at");

            builder.Property(p => p.CompletedAt)
                .HasColumnName("completed_at");

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(p => p.FailureReason)
                .HasColumnName("failure_reason");

            builder.HasIndex(s => s.DomainId);
            builder.HasIndex(s => s.Status);
            builder.HasIndex(p => p.CreatedAt);
        }
    }
}