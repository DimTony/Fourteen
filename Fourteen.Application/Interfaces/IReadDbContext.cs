using Fourteen.Domain.Aggregates.Profiles;
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
    }
}
