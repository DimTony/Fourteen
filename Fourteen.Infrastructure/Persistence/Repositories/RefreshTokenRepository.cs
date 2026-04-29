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
using Fourteen.Domain.Aggregates.Users;

namespace Fourteen.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository
            : Repository<RefreshToken, RefreshTokenId>,
              IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context) { }

        public async Task<RefreshToken?> FindValidByUser(string rawToken, CancellationToken ct = default)
        {
            return await _context.RefreshTokens.AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == rawToken && rt.ExpiresAt > DateTime.UtcNow, ct);
        }

        public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.RefreshTokens.AsNoTracking()
                .Where(rt => rt.UserId == userId && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);
        }
    }
}