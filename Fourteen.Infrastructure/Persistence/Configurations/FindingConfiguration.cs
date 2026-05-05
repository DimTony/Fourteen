using Fourteen.Domain.Aggregates.Domains;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fourteen.Infrastructure.Persistence.Configurations
{
    public class FindingConfiguration : IEntityTypeConfiguration<Finding>
    {
        public void Configure(EntityTypeBuilder<Finding> builder)
        {
            builder.ToTable("findings");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasConversion(id => id.Value, v => new FindingId(v))
                .HasColumnName("id");

            builder.Property(s => s.ScanId)
                .HasConversion(id => id.Value, v => new ScanId(v))
                .HasColumnName("scan_id");

            builder.Property(s => s.Type)
                .HasConversion<string>()
                .HasColumnName("type");

            builder.Property(s => s.Severity)
                .HasConversion<string>()
                .HasColumnName("severity");

            builder.Property(p => p.Title)
                .IsRequired()
                .HasColumnName("title");

            builder.Property(p => p.RawData)
                .IsRequired()
                .HasColumnName("raw_data");

            builder.Property(p => p.AiExplanation)
                .IsRequired()
                .HasColumnName("explanation");
            
            builder.Property(p => p.AiRecommendation)
                .IsRequired()
                .HasColumnName("recommendation");

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.HasIndex(s => s.ScanId);
            builder.HasIndex(s => s.Type);
            builder.HasIndex(s => s.Severity);
            builder.HasIndex(p => p.CreatedAt);
        }
    }
}