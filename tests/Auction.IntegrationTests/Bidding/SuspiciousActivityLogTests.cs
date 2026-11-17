// Suspicious activity journal on a real PostgreSQL: entries are stored with details and listed by type, newest first.
using Auction.Core.Bidding.Contracts;
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.IntegrationTests.Infrastructure;
using Auction.IntegrationTests.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Auction.IntegrationTests.Bidding;

[Collection(PostgresCollection.Name)]
public class SuspiciousActivityLogTests(PostgresApiFixture fixture) : IAsyncLifetime
{
    private static readonly PageRequest FirstPage = new(1, 50);

    private readonly DatabaseTestHelper database = new(fixture);

    public Task InitializeAsync() => database.MigrateAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Recorded_entry_is_listed_with_its_details()
    {
        var user = await database.NewUserAsync();
        var lot = await database.NewLotAsync();

        await RecordAsync(SuspiciousActivityType.RateLimit, user.Id, lot.Id, new Dictionary<string, object?> { ["attempts"] = 4 });

        var entry = Assert.Single(await ListForUserAsync(SuspiciousActivityType.RateLimit, user.Id));
        Assert.Equal(lot.Id, entry.LotId);
        Assert.Equal("4", entry.Details["attempts"]?.ToString());
        Assert.False(entry.IsReviewed);
    }

    [Fact]
    public async Task Entry_without_a_lot_is_stored()
    {
        var user = await database.NewUserAsync();

        await RecordAsync(SuspiciousActivityType.LoginFailures, user.Id, lotId: null, new Dictionary<string, object?>());

        var entry = Assert.Single(await ListForUserAsync(SuspiciousActivityType.LoginFailures, user.Id));
        Assert.Null(entry.LotId);
        Assert.Empty(entry.Details);
    }

    [Fact]
    public async Task List_returns_only_the_requested_type()
    {
        var user = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        await RecordAsync(SuspiciousActivityType.OwnLotBid, user.Id, lot.Id, new Dictionary<string, object?>());
        await RecordAsync(SuspiciousActivityType.NewAccountBid, user.Id, lot.Id, new Dictionary<string, object?>());

        var page = await ListAsync(SuspiciousActivityType.OwnLotBid, FirstPage);

        Assert.All(page.Items, entry => Assert.Equal(SuspiciousActivityType.OwnLotBid, entry.Type));
        Assert.Single(page.Items, entry => entry.UserId == user.Id);
    }

    [Fact]
    public async Task List_puts_the_newest_entries_first()
    {
        var user = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        await RecordAsync(SuspiciousActivityType.OwnLotBid, user.Id, lot.Id, new Dictionary<string, object?> { ["order"] = 1 });
        await RecordAsync(SuspiciousActivityType.OwnLotBid, user.Id, lot.Id, new Dictionary<string, object?> { ["order"] = 2 });

        var page = await ListAsync(SuspiciousActivityType.OwnLotBid, FirstPage);

        Assert.Equal(page.Items.OrderByDescending(entry => entry.CreatedAt).Select(entry => entry.Id), page.Items.Select(entry => entry.Id));
        Assert.Equal(["2", "1"], page.Items.Where(entry => entry.UserId == user.Id).Select(entry => entry.Details["order"]?.ToString()));
    }

    [Fact]
    public async Task Page_size_limits_the_items_but_not_the_total()
    {
        var user = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        for (var i = 0; i < 3; i++)
        {
            await RecordAsync(SuspiciousActivityType.NewAccountBid, user.Id, lot.Id, new Dictionary<string, object?>());
        }

        var page = await ListAsync(SuspiciousActivityType.NewAccountBid, new PageRequest(1, 2));

        Assert.Equal(2, page.Items.Count);
        Assert.True(page.TotalCount >= 3);
    }

    private async Task RecordAsync(SuspiciousActivityType type, long userId, long? lotId, Dictionary<string, object?> details)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var log = scope.ServiceProvider.GetRequiredService<ISuspiciousActivityLog>();
        await log.RecordAsync(type, userId, lotId, details, CancellationToken.None);
    }

    private async Task<PagedList<SuspiciousActivityEntry>> ListAsync(SuspiciousActivityType? type, PageRequest page)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var log = scope.ServiceProvider.GetRequiredService<ISuspiciousActivityLog>();
        return await log.ListAsync(type, page, CancellationToken.None);
    }

    private async Task<List<SuspiciousActivityEntry>> ListForUserAsync(SuspiciousActivityType type, long userId)
    {
        var page = await ListAsync(type, FirstPage);
        return page.Items.Where(entry => entry.UserId == userId).ToList();
    }
}
