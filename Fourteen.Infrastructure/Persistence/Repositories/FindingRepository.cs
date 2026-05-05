using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using Fourteen.Domain.Aggregates.Domains;


namespace Fourteen.Infrastructure.Persistence.Repositories
{
    public class FindingRepository
            : Repository<Finding, FindingId>,
              IFindingRepository
    {
        public FindingRepository(AppDbContext context) : base(context) { }

    }
}