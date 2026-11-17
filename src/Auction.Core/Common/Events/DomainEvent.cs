// Base type for domain events raised by entities and dispatched on SaveChanges.
namespace Auction.Core.Common.Events;

public abstract record DomainEvent(DateTimeOffset OccurredAt);
