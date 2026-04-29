using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Aggregates.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Interfaces
{
    public interface IReadDbContext
    {
        IQueryable<Profile> Profiles { get; }
        IQueryable<User> Users { get; }
        IQueryable<RefreshToken> RefreshTokens { get; }
    }
}
