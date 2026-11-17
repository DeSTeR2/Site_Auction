// Collects entity events on SaveChanges: runs in-transaction observers before the write, after-commit ones once committed.
using Auction.Core.Common.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Auction.Infrastructure.Persistence.DomainEvents;

public sealed class DomainEventsInterceptor(DomainEventDispatcher dispatcher, PendingAfterCommitEvents pending)
    : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchInTransactionAsync(eventData.Context, cancellationToken);
        }

        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context?.Database.CurrentTransaction is null)
        {
            foreach (var domainEvent in pending.TakeAll())
            {
                await dispatcher.DispatchAfterCommitAsync(domainEvent, cancellationToken);
            }
        }

        return result;
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        pending.Clear();
        return Task.CompletedTask;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) =>
        SavingChangesAsync(eventData, result).AsTask().GetAwaiter().GetResult();

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result) =>
        SavedChangesAsync(eventData, result).AsTask().GetAwaiter().GetResult();

    public override void SaveChangesFailed(DbContextErrorEventData eventData) => pending.Clear();

    private async Task DispatchInTransactionAsync(DbContext context, CancellationToken ct)
    {
        while (true)
        {
            var raised = context.ChangeTracker.Entries<IHasDomainEvents>()
                .SelectMany(entry =>
                {
                    var events = entry.Entity.DomainEvents.ToList();
                    entry.Entity.ClearDomainEvents();
                    return events;
                })
                .ToList();

            if (raised.Count == 0)
            {
                return;
            }

            foreach (var domainEvent in raised)
            {
                await dispatcher.DispatchInTransactionAsync(domainEvent, ct);
                pending.Add(domainEvent);
            }
        }
    }
}
