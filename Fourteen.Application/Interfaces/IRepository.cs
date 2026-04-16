using Fourteen.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Interfaces
{
    public interface IRepository<TAggregate, TId>
     where TAggregate : AggregateRoot<TId>
    {
        Task<TAggregate?> GetByIdAsync(TId id, CancellationToken ct = default);
        Task AddAsync(TAggregate aggregate, CancellationToken ct = default);
        Task UpdateAsync(TAggregate aggregate, CancellationToken ct = default);
        Task DeleteAsync(TAggregate aggregate, CancellationToken ct = default);
    }
}
