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
    public class UserRepository
            : Repository<User, UserId>,
              IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }
        
        public Task<User?> FindByGithubId(string githubId, CancellationToken ct = default)
        {
            return _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.GithubId == githubId, ct);
        }

        public async Task<MetricDto> GetUserGrowthAsync(CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            var totalUsers = await _context.Users.CountAsync(ct);

            var todayUsers = await _context.Users
                .Where(u => u.CreatedAt >= today && u.CreatedAt < today.AddDays(1))
                .CountAsync(ct);

            var yesterdayUsers = await _context.Users
                .Where(u => u.CreatedAt >= yesterday && u.CreatedAt < today)
                .CountAsync(ct);

            double percentageIncrease = 0;

            if (yesterdayUsers > 0)
            {
                percentageIncrease =
                    ((double)(todayUsers - yesterdayUsers) / yesterdayUsers) * 100;
            }

            return new MetricDto
            {
                Label = "Total Users",
                Value = totalUsers.ToString(),
                Change = $"+{percentageIncrease:F2}%",
                Icon = "User",
                Color = "bg-blue-500"
            };
        }
    }
}