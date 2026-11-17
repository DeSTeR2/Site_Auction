// Marks an entity that collects domain events until the unit of work is saved.
namespace Auction.Core.Common.Events;

public interface IHasDomainEvents
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
