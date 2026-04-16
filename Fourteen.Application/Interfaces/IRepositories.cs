using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
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

    }
}
