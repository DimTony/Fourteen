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
            var candidates = await _context.RefreshTokens
                .Where(rt => !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);

            return candidates.FirstOrDefault(rt =>
                BCrypt.Net.BCrypt.Verify(rawToken, rt.Token));
        }

        public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
            Guid userId, CancellationToken ct = default)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.ExpiresAt > DateTime.UtcNow && !rt.IsRevoked)
                .ToListAsync(ct);
        }
    }
}