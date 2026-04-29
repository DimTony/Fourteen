using Fourteen.Application.Interfaces;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;
using Fourteen.Application.Common.DTOs;
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
            return _context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Name == name, ct);
        }

        public Task<List<Profile>> GetAll(string? gender, string? countryId, string? ageGroup, CancellationToken ct = default)
        {
            var query = _context.Profiles.AsNoTracking().AsQueryable();

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

        public async Task<(IReadOnlyList<Profile>, int)> NaturalLanguageSearch(ParsedProfileFilter filter, int page = 1, int limit = 10, CancellationToken ct = default)
        {
            var query = _context.Profiles.AsNoTracking().AsQueryable();

            if (filter.Gender is not null)
                query = query.Where(p => p.Gender == filter.Gender);

            if (filter.AgeMin is not null)
                query = query.Where(p => p.Age >= filter.AgeMin);

            if (filter.AgeMax is not null)
                query = query.Where(p => p.Age <= filter.AgeMax);

            if (filter.CountryId is not null)
                query = query.Where(p => p.CountryId == filter.CountryId);

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .OrderBy(p => p.Name)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<(IReadOnlyList<Profile>, int)> GetPagedAsync(GetProfilesQuery q, CancellationToken ct = default)
        {
            var query = _context.Profiles.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(q.Gender))        query = query.Where(p => p.Gender == q.Gender);
            if (!string.IsNullOrEmpty(q.AgeGroup))      query = query.Where(p => p.AgeGroup == q.AgeGroup);
            if (!string.IsNullOrEmpty(q.CountryId))     query = query.Where(p => p.CountryId == q.CountryId);
            if (q.MinAge.HasValue)                      query = query.Where(p => p.Age >= q.MinAge.Value);
            if (q.MaxAge.HasValue)                      query = query.Where(p => p.Age <= q.MaxAge.Value);
            if (q.MinGenderProbability.HasValue)        query = query.Where(p => p.GenderProbability >= q.MinGenderProbability.Value);
            if (q.MaxGenderProbability.HasValue)        query = query.Where(p => p.GenderProbability <= q.MaxGenderProbability.Value);
            if (q.MinCountryProbability.HasValue)       query = query.Where(p => p.CountryProbability >= q.MinCountryProbability.Value);

            var total = await query.CountAsync(ct);

            query = (q.SortBy, q.Order) switch
            {
                ("age", "asc")                    => query.OrderBy(p => p.Age),
                ("age", "desc")                   => query.OrderByDescending(p => p.Age),
                ("gender_probability", "asc")     => query.OrderBy(p => p.GenderProbability),
                ("gender_probability", "desc")    => query.OrderByDescending(p => p.GenderProbability),
                ("created_at", "desc")            => query.OrderByDescending(p => p.CreatedAt),
                _                                 => query.OrderBy(p => p.CreatedAt),
            };

            var items = await query
                .Skip((q.Page - 1) * q.Limit)
                .Take(q.Limit)
                .ToListAsync(ct);

            return (items, total);
        }
    }

}
