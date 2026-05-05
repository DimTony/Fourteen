
using Fourteen.Domain.Common;

namespace Fourteen.Application.Common.Interfaces
{
    public interface IDomainEventCollector
    {
        IEnumerable<DomainEvent> CollectAndClear();
    }
}
