using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Aggregates.Users;

namespace Fourteen.Application.Interfaces
{
    public interface IReadDbContext
    {
        IQueryable<Profile> Profiles { get; }
        IQueryable<User> Users { get; }
        IQueryable<RefreshToken> RefreshTokens { get; }
    }
}
