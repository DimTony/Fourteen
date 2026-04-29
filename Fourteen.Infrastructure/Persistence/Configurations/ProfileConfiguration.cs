using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fourteen.Infrastructure.Persistence.Configurations
{
    public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
    {
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.ToTable("profiles");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasConversion(id => id.Value, v => new ProfileId(v))
                .HasColumnName("id");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");
            builder.HasIndex(p => p.Name).IsUnique();

            builder.Property(p => p.Gender)
                .IsRequired()
                .HasColumnName("gender");

            builder.Property(p => p.GenderProbability)
                .HasColumnName("gender_probability");

            builder.Property(p => p.SampleSize)
                .IsRequired()
                .HasColumnName("sample_size");

            builder.Property(p => p.Age)
                .HasColumnName("age");

            builder.Property(p => p.AgeGroup)
                .IsRequired()
                .HasColumnName("age_group");

            builder.Property(p => p.CountryId)
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnName("country_id");

            builder.Property(p => p.CountryName)
                .IsRequired()
                .HasColumnName("country_name");

            builder.Property(p => p.CountryProbability)
                .HasColumnName("country_probability");

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at");

            builder.HasIndex(p => new { p.Gender, p.CreatedAt });
            builder.HasIndex(p => new { p.AgeGroup, p.CreatedAt });
            builder.HasIndex(p => new { p.CountryId, p.CreatedAt });

            builder.HasIndex(p => p.Age);
            builder.HasIndex(p => p.GenderProbability);
            builder.HasIndex(p => p.CreatedAt);
        }
    }
}
