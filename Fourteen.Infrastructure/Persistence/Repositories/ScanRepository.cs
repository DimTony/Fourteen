using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Fourteen.Domain.Aggregates.Users;
using DomainEntity = Fourteen.Domain.Aggregates.Domains.Domain;
using Fourteen.Application.Features.Domains.Queries.GetDomains;
using Fourteen.Domain.Aggregates.Domains;


namespace Fourteen.Infrastructure.Persistence.Repositories
{
    public class ScanRepository
            : Repository<Scan, ScanId>,
              IScanRepository
    {
        public ScanRepository(AppDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Scan>> GetActiveByDomain(DomainId domainId, CancellationToken ct = default)
        {
            var query = _context.Scans.AsNoTracking().AsQueryable();

            return await query
                .Where(s =>
                        s.DomainId == domainId &&
                        (s.Status == ScanStatus.Pending || s.Status == ScanStatus.Running))
                .ToListAsync(ct);
        }
    }
}