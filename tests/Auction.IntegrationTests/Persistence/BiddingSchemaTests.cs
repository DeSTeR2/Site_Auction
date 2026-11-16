// Bidding tables on a real PostgreSQL: the UNIQUE (lot_id, amount) safety net, amount checks and the email outbox columns.
using Auction.Core.Bidding.Domain;
using Auction.Core.Common.Enums;
using Auction.IntegrationTests.Infrastructure;
using Npgsql;
using static Auction.IntegrationTests.Persistence.DatabaseTestHelper;

namespace Auction.IntegrationTests.Persistence;

[Collection(PostgresCollection.Name)]
public class BiddingSchemaTests(PostgresApiFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseTestHelper database = new(fixture);

    public Task InitializeAsync() => database.MigrateAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Migration_creates_all_bidding_tables()
    {
        var tables = await database.QueryColumnAsync("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'");

        foreach (var expected in new[] { "bids", "notifications", "suspicious_activities" })
        {
            Assert.Contains(expected, tables);
        }
    }

    [Fact]
    public async Task Bid_round_trips()
    {
        var bidder = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        var bid = Bid.Place(lot.Id, bidder.Id, 500, Now);
        await database.SaveAsync(bid);

        var loaded = await database.LoadAsync<Bid>(bid.Id);

        Assert.Equal(500, loaded.Amount);
        Assert.Equal(bidder.Id, loaded.BidderId);
    }

    [Fact]
    public async Task Same_amount_on_the_same_lot_is_rejected_even_for_another_bidder()
    {
        var first = await database.NewUserAsync();
        var second = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        await database.SaveAsync(Bid.Place(lot.Id, first.Id, 600, Now));

        await AssertViolationAsync(() => database.SaveAsync(Bid.Place(lot.Id, second.Id, 600, Now)), PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Same_amount_on_another_lot_is_allowed()
    {
        var bidder = await database.NewUserAsync();
        var firstLot = await database.NewLotAsync();
        var secondLot = await database.NewLotAsync();
        await database.SaveAsync(Bid.Place(firstLot.Id, bidder.Id, 600, Now));

        await database.SaveAsync(Bid.Place(secondLot.Id, bidder.Id, 600, Now));
    }

    [Fact]
    public async Task Parallel_bids_with_the_same_amount_leave_exactly_one_row()
    {
        var lot = await database.NewLotAsync();
        var bidders = new List<long>();
        for (var i = 0; i < 20; i++)
        {
            bidders.Add((await database.NewUserAsync()).Id);
        }

        var attempts = bidders
            .Select(bidderId => Task.Run(async () =>
            {
                try
                {
                    await database.SaveAsync(Bid.Place(lot.Id, bidderId, 700, Now));
                    return true;
                }
                catch (Exception exception) when ((exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return false;
                }
            }))
            .ToList();

        var results = await Task.WhenAll(attempts);

        Assert.Single(results, accepted => accepted);
        Assert.Equal(["1"], await database.QueryColumnAsync($"SELECT count(*)::text FROM bids WHERE lot_id = {lot.Id} AND amount = 700"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(100_000_001)]
    public async Task Amount_outside_the_allowed_range_is_rejected(long amount)
    {
        var bidder = await database.NewUserAsync();
        var lot = await database.NewLotAsync();

        await AssertViolationAsync(() => database.SaveAsync(Bid.Place(lot.Id, bidder.Id, amount, Now)), PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Bid_on_an_unknown_lot_is_rejected()
    {
        var bidder = await database.NewUserAsync();

        await AssertViolationAsync(() => database.SaveAsync(Bid.Place(900_000_002, bidder.Id, 500, Now)), PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task Bid_from_an_unknown_user_is_rejected()
    {
        var lot = await database.NewLotAsync();

        await AssertViolationAsync(() => database.SaveAsync(Bid.Place(lot.Id, 900_000_003, 500, Now)), PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task Notification_is_stored_with_snake_case_type_and_pending_email()
    {
        var recipient = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        var notification = Notification.New(recipient.Id, NotificationType.NewBidOnYourLot, lot.Id, "text", $"/lots/{lot.Id}", EmailStatus.Pending, Now);
        await database.SaveAsync(notification);

        var stored = await database.QueryColumnAsync(
            $"SELECT type || '/' || email_status || '/' || is_read::text FROM notifications WHERE id = {notification.Id}");

        Assert.Equal(["new_bid_on_your_lot/pending/false"], stored);
    }

    [Fact]
    public async Task More_than_three_email_attempts_are_rejected()
    {
        var recipient = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        var notification = Notification.New(recipient.Id, NotificationType.Outbid, lot.Id, "text", "/lots/1", EmailStatus.Pending, Now);
        await database.SaveAsync(notification);

        await AssertViolationAsync(
            () => database.ExecuteAsync($"UPDATE notifications SET email_attempts = 4 WHERE id = {notification.Id}"),
            PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Only_pending_emails_are_in_the_outbox_index()
    {
        var definitions = await database.QueryColumnAsync(
            "SELECT indexdef FROM pg_indexes WHERE tablename = 'notifications' AND indexname LIKE '%email_next_attempt_at%'");

        Assert.Single(definitions);
        Assert.Contains("email_status = 'pending'", definitions[0]);
    }

    [Fact]
    public async Task Deleting_a_user_removes_their_notifications()
    {
        var recipient = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        await database.SaveAsync(Notification.New(recipient.Id, NotificationType.YouWon, lot.Id, "text", "/lots/1", EmailStatus.None, Now));

        await database.ExecuteAsync($"DELETE FROM users WHERE id = {recipient.Id}");

        Assert.Equal(["0"], await database.QueryColumnAsync($"SELECT count(*)::text FROM notifications WHERE recipient_id = {recipient.Id}"));
    }

    [Fact]
    public async Task Suspicious_activity_details_are_queryable_jsonb()
    {
        var user = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        var activity = SuspiciousActivity.Record(
            SuspiciousActivityType.RateLimit,
            user.Id,
            lot.Id,
            new Dictionary<string, object?> { ["attempts"] = 4 },
            Now);
        await database.SaveAsync(activity);

        var stored = await database.QueryColumnAsync(
            $"SELECT type || '/' || (details->>'attempts') FROM suspicious_activities WHERE id = {activity.Id}");

        Assert.Equal(["rate_limit/4"], stored);
    }

    [Fact]
    public async Task Suspicious_activity_with_unknown_type_is_rejected()
    {
        var user = await database.NewUserAsync();

        await AssertViolationAsync(
            () => database.ExecuteAsync(
                $"INSERT INTO suspicious_activities (user_id, type, details, created_at, is_reviewed) VALUES ({user.Id}, 'unknown', '{{}}', now(), false)"),
            PostgresErrorCodes.CheckViolation);
    }
}
