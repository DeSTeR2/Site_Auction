// Resolves observers of a domain event from DI and invokes them in-transaction or after commit.
using System.Reflection;
using Auction.Core.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auction.Infrastructure.Persistence.DomainEvents;

public sealed class DomainEventDispatcher(IServiceProvider services, ILogger<DomainEventDispatcher> logger)
{
    public async Task DispatchInTransactionAsync(DomainEvent domainEvent, CancellationToken ct)
    {
        var handlerType = typeof(IInTransactionHandler<>).MakeGenericType(domainEvent.GetType());
        foreach (var handler in services.GetServices(handlerType))
        {
            await InvokeAsync(handlerType, handler!, domainEvent, ct);
        }
    }

    public async Task DispatchAfterCommitAsync(DomainEvent domainEvent, CancellationToken ct)
    {
        var handlerType = typeof(IAfterCommitHandler<>).MakeGenericType(domainEvent.GetType());
        foreach (var handler in services.GetServices(handlerType))
        {
            try
            {
                await InvokeAsync(handlerType, handler!, domainEvent, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "After-commit handler {Handler} failed for {Event}",
                    handler!.GetType().Name,
                    domainEvent.GetType().Name);
            }
        }
    }

    private static Task InvokeAsync(Type handlerType, object handler, DomainEvent domainEvent, CancellationToken ct)
    {
        var method = handlerType.GetMethod(nameof(IInTransactionHandler<DomainEvent>.HandleAsync))!;
        return (Task)method.Invoke(handler, BindingFlags.DoNotWrapExceptions, null, [domainEvent, ct], null)!;
    }
}
