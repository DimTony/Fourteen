using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;
using Fourteen.Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Interfaces
{
    public interface IProfileRepository
: IRepository<Profile, ProfileId>
    {
        Task<Profile?> GetByName(string name, CancellationToken ct = default);
        Task<List<Profile>> GetAll(string? gender, string? countryId, string? ageGroup, CancellationToken ct = default);
        Task<(IEnumerable<Profile>, int)> NaturalLanguageSearch(ParsedProfileFilter filter, int page = 1, int limit = 10, CancellationToken ct = default);
        Task<(IEnumerable<Profile>, int)> GetPagedAsync(GetProfilesQuery q, CancellationToken ct = default);

    }
}
