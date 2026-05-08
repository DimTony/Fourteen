using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Fourteen.Domain.Aggregates.Users;
using DomainEntity = Fourteen.Domain.Aggregates.Domains.Domain;
using Fourteen.Application.Features.Domains.Queries.GetDomains;


namespace Fourteen.Infrastructure.Persistence.Repositories
{
    public class DomainRepository
            : Repository<DomainEntity, DomainId>,
              IDomainRepository
    {
        public DomainRepository(AppDbContext context) : base(context) { }

        public Task<DomainEntity?> GetByNameAndUser(UserId userId, string name, CancellationToken ct = default)
        {
            return _context.Domains.AsNoTracking().FirstOrDefaultAsync(d => d.OwnerId == userId && d.Name == name, ct);
        }

        public async Task<(IReadOnlyList<DomainEntity>, int)> GetPaged(GetDomainsQuery q, CancellationToken ct = default)
        {
            var query = _context.Domains.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(q.Status))
            {
                if (Enum.TryParse<VerificationStatus>(q.Status, true, out var status))
                {
                    query = query.Where(d => d.VerificationStatus == status);
                }
            }

            if (q.UserId.HasValue)
            {
                query = query.Where(d => d.OwnerId == new UserId(q.UserId.Value));
            }

            var total = await query.CountAsync(ct);

            query = (q.SortBy, q.Order) switch
            {
                ("verified_at", "asc")     => query.OrderBy(p => p.VerifiedAt),
                ("verified_at", "desc")    => query.OrderByDescending(p => p.VerifiedAt),
                ("created_at", "desc")     => query.OrderByDescending(p => p.CreatedAt),
                _                          => query.OrderBy(p => p.CreatedAt),
            };

            var items = await query
                .Skip((q.Page - 1) * q.Limit)
                .Take(q.Limit)
                .ToListAsync(ct);

            return (items, total);
            
        }
    }
}