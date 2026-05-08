using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;
using Fourteen.Application.Common.DTOs;
using Fourteen.Domain.Aggregates.Users;

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
        Task<User?> FindByEmail(string email, CancellationToken ct = default);
        Task<User?> FindByProviderId(string providerId, CancellationToken ct = default);

        Task<MetricDto> GetUserGrowth(CancellationToken ct = default);
    }

    public interface IRefreshTokenRepository
        : IRepository<RefreshToken, RefreshTokenId>
    {
        Task<RefreshToken?> FindValidByUser(string rawToken, CancellationToken ct = default);

        Task<IReadOnlyList<RefreshToken>> GetActiveByUserId(Guid userId, CancellationToken ct = default);
    } 

}
