// Lots and charity tables on a real PostgreSQL: seed, value checks, foreign keys, cascades and search indexes.
using Auction.Core.Common.Enums;
using Auction.Core.Lots.Domain;
using Auction.IntegrationTests.Infrastructure;
using Npgsql;
using static Auction.IntegrationTests.Persistence.DatabaseTestHelper;

namespace Auction.IntegrationTests.Persistence;

[Collection(PostgresCollection.Name)]
public class LotsAndCharitySchemaTests(PostgresApiFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseTestHelper database = new(fixture);

    public Task InitializeAsync() => database.MigrateAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Migration_creates_all_lots_and_charity_tables()
    {
        var tables = await database.QueryColumnAsync("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'");

        foreach (var expected in new[] { "categories", "lots", "lot_photos", "favorites", "lot_views", "donation_links", "donation_reports" })
        {
            Assert.Contains(expected, tables);
        }
    }

    [Fact]
    public async Task Eleven_categories_are_seeded_in_display_order()
    {
        var names = await database.QueryColumnAsync("SELECT name FROM categories ORDER BY sort_order");

        Assert.Equal(11, names.Count);
        Assert.Equal("Монети і банкноти", names[0]);
        Assert.Equal("Інше", names[^1]);
    }

    [Fact]
    public async Task Draft_lot_round_trips_with_enums_as_snake_case_text()
    {
        var lot = await database.NewLotAsync(input: DraftInput() with { Condition = LotCondition.LikeNew });

        var stored = await database.QueryColumnAsync($"SELECT status || '/' || condition FROM lots WHERE id = {lot.Id}");
        var loaded = await database.LoadAsync<Lot>(lot.Id);

        Assert.Equal(["draft/like_new"], stored);
        Assert.Equal(LotStatus.Draft, loaded.Status);
        Assert.Equal(500, loaded.CurrentPrice);
    }

    [Fact]
    public async Task Start_price_of_zero_is_rejected()
    {
        var seller = await database.NewUserAsync();

        await AssertViolationAsync(
            () => database.NewLotAsync(seller.Id, DraftInput() with { StartPrice = 0 }),
            PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Title_shorter_than_five_characters_is_rejected()
    {
        var seller = await database.NewUserAsync();

        await AssertViolationAsync(
            () => database.NewLotAsync(seller.Id, DraftInput() with { Title = "abcd" }),
            PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Lot_with_unknown_category_is_rejected()
    {
        var seller = await database.NewUserAsync();

        await AssertViolationAsync(
            () => database.NewLotAsync(seller.Id, DraftInput(categoryId: 99)),
            PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task Active_lot_without_start_and_end_time_is_rejected()
    {
        var lot = await database.NewLotAsync();

        await AssertViolationAsync(
            () => database.ExecuteAsync($"UPDATE lots SET status = 'active' WHERE id = {lot.Id}"),
            PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Final_price_is_allowed_only_on_a_closed_lot()
    {
        var lot = await database.NewLotAsync();

        await AssertViolationAsync(
            () => database.ExecuteAsync($"UPDATE lots SET final_price = 600 WHERE id = {lot.Id}"),
            PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Title_and_description_have_trigram_indexes()
    {
        var indexes = await database.QueryColumnAsync(
            "SELECT indexname FROM pg_indexes WHERE tablename = 'lots' AND indexdef LIKE '%gin_trgm_ops%' ORDER BY indexname");

        Assert.Equal(2, indexes.Count);
    }

    [Fact]
    public async Task Favorite_is_unique_per_user_and_lot()
    {
        var user = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        await database.SaveAsync(Favorite.Add(user.Id, lot.Id, Now));

        await AssertViolationAsync(() => database.SaveAsync(Favorite.Add(user.Id, lot.Id, Now)), PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Lot_view_is_counted_once_per_visitor_and_day()
    {
        var lot = await database.NewLotAsync();
        var day = new DateOnly(2026, 11, 16);
        await database.SaveAsync(LotView.Record(lot.Id, LotView.AnonymousKey("abc"), day));

        await AssertViolationAsync(
            () => database.SaveAsync(LotView.Record(lot.Id, LotView.AnonymousKey("abc"), day)),
            PostgresErrorCodes.UniqueViolation);
        await database.SaveAsync(LotView.Record(lot.Id, LotView.AnonymousKey("abc"), day.AddDays(1)));
    }

    [Fact]
    public async Task Deleting_a_lot_removes_its_photos_favorites_and_views()
    {
        var user = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        await database.SaveAsync(LotPhoto.Add(lot.Id, 0, "lots/a-large.webp", "lots/a-thumb.webp", 1600, 1200, Now));
        await database.SaveAsync(Favorite.Add(user.Id, lot.Id, Now));
        await database.SaveAsync(LotView.Record(lot.Id, LotView.UserKey(user.Id), new DateOnly(2026, 11, 16)));

        await database.ExecuteAsync($"DELETE FROM lots WHERE id = {lot.Id}");

        foreach (var table in new[] { "lot_photos", "favorites", "lot_views" })
        {
            Assert.Equal(["0"], await database.QueryColumnAsync($"SELECT count(*)::text FROM {table} WHERE lot_id = {lot.Id}"));
        }
    }

    [Fact]
    public async Task Donation_link_url_is_unique()
    {
        var user = await database.NewUserAsync();
        var url = $"https://send.monobank.ua/{Suffix()}";
        await database.SaveAsync(DonationLink.Submit(url, url, user.Id, Now));

        await AssertViolationAsync(
            () => database.SaveAsync(DonationLink.Submit(url, url + "?utm=1", user.Id, Now)),
            PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Approved_donation_link_is_stored_as_text_and_linked_to_a_lot()
    {
        var user = await database.NewUserAsync();
        var admin = await database.NewUserAsync();
        var url = $"https://send.monobank.ua/{Suffix()}";
        var link = DonationLink.Submit(url, url, user.Id, Now);
        await database.SaveAsync(link);
        await database.UpdateAsync<DonationLink>(link.Id, stored => stored.Approve(admin.Id, Now.AddHours(1)));

        var status = await database.QueryColumnAsync($"SELECT status FROM donation_links WHERE id = {link.Id}");
        var lot = await database.NewLotAsync(user.Id);
        await database.ExecuteAsync($"UPDATE lots SET donation_link_id = {link.Id} WHERE id = {lot.Id}");

        Assert.Equal(["approved"], status);
    }

    [Fact]
    public async Task Donation_link_with_unknown_status_is_rejected()
    {
        var user = await database.NewUserAsync();

        await AssertViolationAsync(
            () => database.ExecuteAsync(
                "INSERT INTO donation_links (normalized_url, original_url, status, submitted_by, created_at) " +
                $"VALUES ('https://a.example/{Suffix()}', 'https://a.example', 'unknown', {user.Id}, now())"),
            PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Lot_has_at_most_one_donation_report()
    {
        var winner = await database.NewUserAsync();
        var lot = await database.NewLotAsync();
        await database.SaveAsync(DonationReport.Create(lot.Id, winner.Id, 600, "receipts/a-large.webp", Now));

        await AssertViolationAsync(
            () => database.SaveAsync(DonationReport.Create(lot.Id, winner.Id, 600, null, Now)),
            PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Donation_report_amount_must_be_positive()
    {
        var winner = await database.NewUserAsync();
        var lot = await database.NewLotAsync();

        await AssertViolationAsync(
            () => database.SaveAsync(DonationReport.Create(lot.Id, winner.Id, 0, null, Now)),
            PostgresErrorCodes.CheckViolation);
    }
}
