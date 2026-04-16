using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Infrastructure.Persistence
{
    public class Repository<TAggregate, TId> : IRepository<TAggregate, TId>
        where TAggregate : AggregateRoot<TId>
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TAggregate> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TAggregate>();
        }

        public virtual async Task<TAggregate?> GetByIdAsync(TId id, CancellationToken ct = default) =>
            await _dbSet.FindAsync(new object?[] { id }, ct);

        public virtual async Task AddAsync(TAggregate aggregate, CancellationToken ct = default) =>
            await _dbSet.AddAsync(aggregate, ct);

        public virtual Task UpdateAsync(TAggregate aggregate, CancellationToken ct = default)
        {
            _dbSet.Update(aggregate);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(TAggregate aggregate, CancellationToken ct = default)
        {
            _dbSet.Remove(aggregate);
            return Task.CompletedTask;
        }
    }

}
