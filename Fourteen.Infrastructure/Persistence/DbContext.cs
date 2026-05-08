using Fourteen.Application.Interfaces;
using DomainEntity = Fourteen.Domain.Aggregates.Domains.Domain;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fourteen.Domain.Aggregates.Domains;

namespace Fourteen.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IReadDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Profile> Profiles => Set<Profile>();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<DomainEntity> Domains => Set<DomainEntity>();
        public DbSet<Scan> Scans => Set<Scan>();
        public DbSet<Finding> Findings => Set<Finding>();

        IQueryable<Profile> IReadDbContext.Profiles => Profiles;
        IQueryable<User> IReadDbContext.Users => Users;
        IQueryable<RefreshToken> IReadDbContext.RefreshTokens => RefreshTokens;
        IQueryable<DomainEntity> IReadDbContext.Domains => Domains;
        IQueryable<Scan> IReadDbContext.Scans => Scans;
        IQueryable<Finding> IReadDbContext.Findings => Findings;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<DomainEvent>();

            modelBuilder.Ignore<ProfileId>();
            modelBuilder.Ignore<UserId>();
            modelBuilder.Ignore<RefreshTokenId>();
            modelBuilder.Ignore<DomainId>();
            modelBuilder.Ignore<ScanId>();
            modelBuilder.Ignore<FindingId>();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            RegisterStronglyTypedIdConverters(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void RegisterStronglyTypedIdConverters(ModelBuilder modelBuilder)
        {
            var profileIdConverter = new ValueConverter<ProfileId, Guid>(
              id => id.Value,
              value => new ProfileId(value));

            var userIdConverter = new ValueConverter<UserId, Guid>(
              id => id.Value,
              value => new UserId(value));

            var refreshTokenIdConverter = new ValueConverter<RefreshTokenId, Guid>(
              id => id.Value,
              value => new RefreshTokenId(value));

            var domainIdConverter = new ValueConverter<DomainId, Guid>(
              id => id.Value,
              value => new DomainId(value));

            var scanIdConverter = new ValueConverter<ScanId, Guid>(
              id => id.Value,
              value => new ScanId(value));

            var findingIdConverter = new ValueConverter<FindingId, Guid>(
              id => id.Value,
              value => new FindingId(value));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(ProfileId))
                        property.SetValueConverter(profileIdConverter);
                    else if (property.ClrType == typeof(UserId))
                        property.SetValueConverter(userIdConverter);
                    else if (property.ClrType == typeof(RefreshTokenId))
                        property.SetValueConverter(refreshTokenIdConverter);
                    else if (property.ClrType == typeof(DomainId))
                        property.SetValueConverter(domainIdConverter);
                    else if (property.ClrType == typeof(ScanId))
                        property.SetValueConverter(scanIdConverter);
                    else if (property.ClrType == typeof(FindingId))
                        property.SetValueConverter(findingIdConverter);
                }
            }
        }
    }

}
