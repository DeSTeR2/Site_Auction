// Observer that runs before COMMIT inside the same transaction as the action.
namespace Auction.Core.Common.Events;

public interface IInTransactionHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct);
}
