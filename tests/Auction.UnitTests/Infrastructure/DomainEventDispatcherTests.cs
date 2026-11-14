// Observer dispatch: in-transaction failures propagate, after-commit failures are only logged.
using Auction.Core.Common.Events;
using Auction.Infrastructure.Persistence.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Auction.UnitTests.Infrastructure;

public class DomainEventDispatcherTests
{
    [Fact]
    public async Task In_transaction_handlers_receive_the_event()
    {
        var handler = new RecordingHandler();
        var dispatcher = BuildDispatcher(services => services.AddSingleton<IInTransactionHandler<SampleEvent>>(handler));

        await dispatcher.DispatchInTransactionAsync(new SampleEvent(TestLots.Start), CancellationToken.None);

        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task In_transaction_failure_propagates_to_roll_back()
    {
        var dispatcher = BuildDispatcher(services => services.AddSingleton<IInTransactionHandler<SampleEvent>, FailingHandler>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.DispatchInTransactionAsync(new SampleEvent(TestLots.Start), CancellationToken.None));
    }

    [Fact]
    public async Task After_commit_failure_does_not_stop_other_handlers()
    {
        var handler = new RecordingHandler();
        var dispatcher = BuildDispatcher(services =>
        {
            services.AddSingleton<IAfterCommitHandler<SampleEvent>, FailingHandler>();
            services.AddSingleton<IAfterCommitHandler<SampleEvent>>(handler);
        });

        await dispatcher.DispatchAfterCommitAsync(new SampleEvent(TestLots.Start), CancellationToken.None);

        Assert.Equal(1, handler.Calls);
    }

    private static DomainEventDispatcher BuildDispatcher(Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        register(services);
        return new DomainEventDispatcher(services.BuildServiceProvider(), NullLogger<DomainEventDispatcher>.Instance);
    }

    private sealed record SampleEvent(DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);

    private sealed class RecordingHandler : IInTransactionHandler<SampleEvent>, IAfterCommitHandler<SampleEvent>
    {
        public int Calls { get; private set; }

        public Task HandleAsync(SampleEvent domainEvent, CancellationToken ct)
        {
            Calls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FailingHandler : IInTransactionHandler<SampleEvent>, IAfterCommitHandler<SampleEvent>
    {
        public Task HandleAsync(SampleEvent domainEvent, CancellationToken ct) =>
            throw new InvalidOperationException("Handler failed");
    }
}
