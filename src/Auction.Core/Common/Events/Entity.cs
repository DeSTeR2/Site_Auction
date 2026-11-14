// Base entity with a bigint identity key and a domain event list.
namespace Auction.Core.Common.Events;

public abstract class Entity : IHasDomainEvents
{
    private readonly List<DomainEvent> domainEvents = [];

    public long Id { get; protected set; }

    public IReadOnlyCollection<DomainEvent> DomainEvents => domainEvents;

    public void ClearDomainEvents() => domainEvents.Clear();

    protected void Raise(DomainEvent domainEvent) => domainEvents.Add(domainEvent);
}
