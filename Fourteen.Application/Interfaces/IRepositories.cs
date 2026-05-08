using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;
using Fourteen.Application.Common.DTOs;
using DomainEntity = Fourteen.Domain.Aggregates.Domains.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Application.Features.Domains.Queries.GetDomains;
using Fourteen.Domain.Aggregates.Domains;

namespace Fourteen.Application.Interfaces
{
    public interface IProfileRepository
: IRepository<Profile, ProfileId>
    {
        Task<Profile?> GetByName(string name, CancellationToken ct = default);
        Task<List<Profile>> GetAll(string? gender, string? countryId, string? ageGroup, CancellationToken ct = default);
        Task<(IReadOnlyList<Profile>, int)> NaturalLanguageSearch(ParsedProfileFilter filter, int page = 1, int limit = 10, CancellationToken ct = default);
        Task<(IReadOnlyList<Profile>, int)> GetPaged(GetProfilesQuery q, CancellationToken ct = default);

    }

    public interface IUserRepository
        : IRepository<User, UserId>
    {
        Task<User?> FindByGithubId(string githubId, CancellationToken ct = default);

        Task<MetricDto> GetUserGrowth(CancellationToken ct = default);
    }

    public interface IRefreshTokenRepository
        : IRepository<RefreshToken, RefreshTokenId>
    {
        Task<RefreshToken?> FindValidByUser(string rawToken, CancellationToken ct = default);

        Task<IReadOnlyList<RefreshToken>> GetActiveByUserId(Guid userId, CancellationToken ct = default);
    } 

    public interface IDomainRepository 
        : IRepository<DomainEntity, DomainId>
    {
        Task<DomainEntity?> GetByNameAndUser(UserId userId, string name, CancellationToken ct = default);
        Task<(IReadOnlyList<DomainEntity>, int)> GetPaged(GetDomainsQuery q, CancellationToken ct = default);

    }

    public interface IScanRepository
        : IRepository<Scan, ScanId>
    {
        Task<IReadOnlyList<Scan>> GetActiveByDomain(DomainId domainId, CancellationToken ct = default);
    }

    public interface IFindingRepository
        : IRepository<Finding, FindingId>
    {
        // Task<IReadOnlyList<Scan>> GetActiveByDomain(DomainId domainId, CancellationToken ct = default);
    }
}
