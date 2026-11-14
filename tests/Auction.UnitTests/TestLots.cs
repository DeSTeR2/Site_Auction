// Shared builders for lots in unit tests.
using Auction.Core.Common.Enums;
using Auction.Core.Common.Options;
using Auction.Core.Lots.Domain;
using Microsoft.Extensions.Time.Testing;

namespace Auction.UnitTests;

internal static class TestLots
{
    public const int OneDayMinutes = 24 * 60;

    public static readonly DateTimeOffset Start = new(2026, 12, 1, 10, 0, 0, TimeSpan.Zero);

    public static LotDraftInput Draft(string title = "Вінтажний фотоапарат", int durationMinutes = OneDayMinutes) =>
        new(1, title, "Опис лота довжиною понад двадцять символів", LotCondition.Used, 100, 10, durationMinutes, "https://example.com/fund");

    public static LotFactory Factory(FakeTimeProvider? clock = null, AuctionRules? rules = null) =>
        new(clock ?? new FakeTimeProvider(Start), rules ?? new AuctionRules());

    public static Lot NewDraft(long sellerId = 1, string title = "Вінтажний фотоапарат", int durationMinutes = OneDayMinutes) =>
        Factory().CreateDraft(sellerId, Draft(title, durationMinutes)).Value;

    public static Lot Active(long sellerId = 1, DateTimeOffset? startedAt = null, string title = "Вінтажний фотоапарат", int durationMinutes = OneDayMinutes)
    {
        var lot = NewDraft(sellerId, title, durationMinutes);
        lot.Publish(startedAt ?? Start);
        return lot;
    }
}
