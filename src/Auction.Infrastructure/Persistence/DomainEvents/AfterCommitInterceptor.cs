// Fires buffered after-commit observers when an explicit transaction commits and drops them on rollback.
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Auction.Infrastructure.Persistence.DomainEvents;

public sealed class AfterCommitInterceptor(DomainEventDispatcher dispatcher, PendingAfterCommitEvents pending)
    : DbTransactionInterceptor
{
    public override async Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in pending.TakeAll())
        {
            await dispatcher.DispatchAfterCommitAsync(domainEvent, cancellationToken);
        }
    }

    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData) =>
        TransactionCommittedAsync(transaction, eventData).GetAwaiter().GetResult();

    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData) =>
        pending.Clear();

    public override Task TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        pending.Clear();
        return Task.CompletedTask;
    }
}
