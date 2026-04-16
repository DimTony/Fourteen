using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Infrastructure.Persistence.Repositories
{
    public class ProfileRepository
            : Repository<Profile, ProfileId>,
              IProfileRepository
    {
        public ProfileRepository(AppDbContext context) : base(context) { }


        public Task<Profile?> GetByName(string name, CancellationToken ct = default)
        {
            return _context.Profiles.FirstOrDefaultAsync(p => p.Name == name, ct);
        }

        public Task<List<Profile>> GetAll(string? gender, string? countryId, string? ageGroup, CancellationToken ct = default)
        {
            var query = _context.Profiles.AsQueryable();

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(p => p.Gender == gender);
            }

            if (!string.IsNullOrEmpty(countryId))
            {
                query = query.Where(p => p.CountryId == countryId);
            }

            if (!string.IsNullOrEmpty(ageGroup))
            {
                query = query.Where(p => p.AgeGroup == ageGroup);
            }

            return query.ToListAsync(ct);
        }
    }

}
