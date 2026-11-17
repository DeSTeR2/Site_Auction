// Per-request buffer of events whose after-commit observers wait for the transaction to commit.
using Auction.Core.Common.Events;

namespace Auction.Infrastructure.Persistence.DomainEvents;

public sealed class PendingAfterCommitEvents
{
    private readonly List<DomainEvent> events = [];

    public void Add(DomainEvent domainEvent) => events.Add(domainEvent);

    public IReadOnlyList<DomainEvent> TakeAll()
    {
        var taken = events.ToList();
        events.Clear();
        return taken;
    }

    public void Clear() => events.Clear();
}
