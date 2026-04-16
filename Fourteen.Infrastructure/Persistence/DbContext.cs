using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IReadDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Profile> Profiles => Set<Profile>();

        IQueryable<Profile> IReadDbContext.Profiles => Profiles;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<DomainEvent>();

            modelBuilder.Ignore<ProfileId>();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            RegisterStronglyTypedIdConverters(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void RegisterStronglyTypedIdConverters(ModelBuilder modelBuilder)
        {
            var profileIdConverter = new ValueConverter<ProfileId, Guid>(
              id => id.Value,
              value => new ProfileId(value));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(ProfileId))
                        property.SetValueConverter(profileIdConverter);
                }
            }
        }
    }

}
