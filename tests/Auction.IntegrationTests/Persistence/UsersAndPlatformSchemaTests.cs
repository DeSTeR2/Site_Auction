// Users and Platform tables checked on a real PostgreSQL: conventions, constraints, indexes and the append-only log.
using Auction.Core.Common.Enums;
using Auction.Core.Platform.Contracts;
using Auction.Core.Platform.Domain;
using Auction.Core.Users.Domain;
using Auction.Infrastructure.Persistence;
using Auction.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using static Auction.IntegrationTests.Persistence.DatabaseTestHelper;

namespace Auction.IntegrationTests.Persistence;

[Collection(PostgresCollection.Name)]
public class UsersAndPlatformSchemaTests(PostgresApiFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseTestHelper database = new(fixture);

    public Task InitializeAsync() => database.MigrateAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Migration_creates_all_users_and_platform_tables()
    {
        var tables = await database.QueryColumnAsync("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'");

        foreach (var expected in new[] { "users", "sessions", "user_tokens", "complaints", "admin_actions", "analytics_events" })
        {
            Assert.Contains(expected, tables);
        }
    }

    [Fact]
    public async Task Email_is_unique_ignoring_case()
    {
        var suffix = Suffix();
        await database.SaveAsync(User.Register($"Anna{suffix}@Example.com", $"a{suffix}", "hash", Now));

        var duplicate = User.Register($"anna{suffix}@example.com", $"b{suffix}", "hash", Now);

        await AssertViolationAsync(() => database.SaveAsync(duplicate), PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Nickname_is_unique_ignoring_case()
    {
        var suffix = Suffix();
        await database.SaveAsync(User.Register($"one{suffix}@example.com", $"Nick{suffix}", "hash", Now));

        var duplicate = User.Register($"two{suffix}@example.com", $"nick{suffix}", "hash", Now);

        await AssertViolationAsync(() => database.SaveAsync(duplicate), PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Nickname_shorter_than_three_characters_is_rejected()
    {
        var user = User.Register($"short{Suffix()}@example.com", "ab", "hash", Now);

        await AssertViolationAsync(() => database.SaveAsync(user), PostgresErrorCodes.CheckViolation);
    }

    [Theory]
    [InlineData("0501234567")]
    [InlineData("+38050123456")]
    [InlineData("+380abcdefghi")]
    public async Task Phone_in_wrong_format_is_rejected(string phone)
    {
        var suffix = Suffix();
        var user = User.Register($"phone{suffix}@example.com", $"p{suffix}", "hash", Now);
        user.UpdateProfile($"p{suffix}", phone, null, true);

        await AssertViolationAsync(() => database.SaveAsync(user), PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Profile_with_valid_phone_and_disabled_emails_round_trips()
    {
        var suffix = Suffix();
        var user = User.Register($"profile{suffix}@example.com", $"p{suffix}", "hash", Now);
        user.UpdateProfile($"p{suffix}", "+380501234567", "Львів", emailNotifications: false);
        await database.SaveAsync(user);

        var loaded = await database.LoadAsync<User>(user.Id);

        Assert.Equal("+380501234567", loaded.Phone);
        Assert.Equal("Львів", loaded.City);
        Assert.False(loaded.EmailNotifications);
    }

    [Fact]
    public async Task Enums_are_stored_as_snake_case_text()
    {
        var author = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        var complaint = Complaint.Submit(author.Id, lot.Id, ComplaintReason.FraudOrCounterfeit, null, Now).Value;
        await database.SaveAsync(complaint);

        var stored = await database.QueryColumnAsync($"SELECT reason || '/' || status FROM complaints WHERE id = {complaint.Id}");

        Assert.Equal(["fraud_or_counterfeit/new"], stored);
    }

    [Fact]
    public async Task Second_new_complaint_from_same_author_on_same_lot_is_rejected()
    {
        var author = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        await database.SaveAsync(Complaint.Submit(author.Id, lot.Id, ComplaintReason.FakeBids, null, Now).Value);

        var duplicate = Complaint.Submit(author.Id, lot.Id, ComplaintReason.OffensiveContent, null, Now).Value;

        await AssertViolationAsync(() => database.SaveAsync(duplicate), PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task New_complaint_is_allowed_after_the_previous_one_was_reviewed()
    {
        var author = await database.NewUserAsync();
        var admin = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        var first = Complaint.Submit(author.Id, lot.Id, ComplaintReason.FakeBids, null, Now).Value;
        await database.SaveAsync(first);

        await database.UpdateAsync<Complaint>(first.Id, complaint => complaint.Reject(admin.Id, "not confirmed", Now.AddHours(1)));

        await database.SaveAsync(Complaint.Submit(author.Id, lot.Id, ComplaintReason.FakeBids, null, Now.AddDays(1)).Value);
    }

    [Fact]
    public async Task Reason_other_without_comment_is_rejected_by_the_database()
    {
        var author = await database.NewUserAsync();
        var lot = await database.NewLotAsync();

        await AssertViolationAsync(
            () => database.ExecuteAsync(
                "INSERT INTO complaints (author_id, lot_id, reason, status, created_at) " +
                $"VALUES ({author.Id}, {lot.Id}, 'other', 'new', now())"),
            PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Complaint_about_an_unknown_lot_is_rejected()
    {
        var author = await database.NewUserAsync();
        var complaint = Complaint.Submit(author.Id, 900_000_001, ComplaintReason.FakeBids, null, Now).Value;

        await AssertViolationAsync(() => database.SaveAsync(complaint), PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task Admin_actions_cannot_be_updated_or_deleted()
    {
        var admin = await database.NewUserAsync();
        var action = AdminAction.Record(admin.Id, AdminActionType.HideLot, "lot", 5, "spam", Now);
        await database.SaveAsync(action);

        var update = await Assert.ThrowsAsync<PostgresException>(() => database.ExecuteAsync($"UPDATE admin_actions SET reason = 'x' WHERE id = {action.Id}"));
        var delete = await Assert.ThrowsAsync<PostgresException>(() => database.ExecuteAsync($"DELETE FROM admin_actions WHERE id = {action.Id}"));

        Assert.Contains("append-only", update.MessageText);
        Assert.Contains("append-only", delete.MessageText);
    }

    [Fact]
    public async Task Deleting_a_user_removes_their_sessions_and_tokens()
    {
        var user = await database.NewUserAsync();
        await database.SaveAsync(UserSession.Start(user.Id, Guid.NewGuid().ToByteArray(), Now, Now.AddDays(30), "agent"));
        await database.SaveAsync(UserToken.Issue(user.Id, TokenPurpose.EmailConfirm, Guid.NewGuid().ToByteArray(), Now, TimeSpan.FromHours(24)));

        await database.ExecuteAsync($"DELETE FROM users WHERE id = {user.Id}");

        Assert.Equal(["0"], await database.QueryColumnAsync($"SELECT count(*)::text FROM sessions WHERE user_id = {user.Id}"));
        Assert.Equal(["0"], await database.QueryColumnAsync($"SELECT count(*)::text FROM user_tokens WHERE user_id = {user.Id}"));
    }

    [Fact]
    public async Task Session_token_hash_is_unique()
    {
        var user = await database.NewUserAsync();
        var hash = Guid.NewGuid().ToByteArray();
        await database.SaveAsync(UserSession.Start(user.Id, hash, Now, Now.AddDays(30), null));

        var duplicate = UserSession.Start(user.Id, hash, Now, Now.AddDays(30), null);

        await AssertViolationAsync(() => database.SaveAsync(duplicate), PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Analytics_payload_is_stored_as_queryable_jsonb()
    {
        var record = AnalyticsRecord.From(
            new AnalyticsEvent("lot_published", LotId: 3, Payload: new Dictionary<string, object?> { ["startPrice"] = 500 }),
            visitorKey: null,
            Now);
        await database.SaveAsync(record);

        var stored = await database.QueryColumnAsync($"SELECT payload->>'startPrice' FROM analytics_events WHERE id = {record.Id}");

        Assert.Equal(["500"], stored);
    }
}
