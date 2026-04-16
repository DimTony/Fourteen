using MediatR;

namespace Fourteen.Domain.Common
{
    public abstract record DomainEvent : INotification
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
    public abstract class Entity<TId>
    {
        public TId Id { get; protected set; } = default!;

        private readonly List<DomainEvent> _domainEvents = new();
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void RaiseDomainEvent(DomainEvent domainEvent) =>
            _domainEvents.Add(domainEvent);

        public void ClearDomainEvents() =>
            _domainEvents.Clear();

        public override bool Equals(object? obj)
        {
            if (obj is not Entity<TId> other) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;
            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode() =>
            HashCode.Combine(GetType(), Id);
    }
    public interface IHasDomainEvents
    {
        IReadOnlyCollection<DomainEvent> DomainEvents { get; }
        void ClearDomainEvents();
    }
    public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    {
    }
}
