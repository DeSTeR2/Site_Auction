// Observer that runs after COMMIT for external effects; its failures never roll back the action.
namespace Auction.Core.Common.Events;

public interface IAfterCommitHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct);
}
