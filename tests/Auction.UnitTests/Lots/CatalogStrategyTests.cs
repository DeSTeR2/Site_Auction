// Catalog sort strategies, time filters and the resolver that picks them.
using Auction.Core.Lots.Application.Catalog;
using Auction.Core.Lots.Domain;

namespace Auction.UnitTests.Lots;

public class CatalogStrategyTests
{
    private static readonly CatalogStrategyResolver Resolver = new(
        [new EndingSoonSort(), new NewestSort(), new CheapestSort(), new PriciestSort(), new MostBidsSort(), new RecentlyEndedSort(), new RelevanceSort()],
        [new EndsWithinHourFilter(), new EndsWithinDayFilter(), new EndsWithinThreeDaysFilter(), new EndsWithinAnyFilter()]);

    [Fact]
    public void Default_sort_is_ending_soon_and_text_switches_to_relevance()
    {
        Assert.Equal(CatalogSort.EndingSoon, Resolver.ResolveSort(null, null).Key);
        Assert.Equal(CatalogSort.Relevance, Resolver.ResolveSort(null, "камера").Key);
        Assert.Equal(CatalogSort.MostBids, Resolver.ResolveSort(CatalogSort.MostBids, "камера").Key);
    }

    [Fact]
    public void Ending_soon_orders_by_end_time()
    {
        var later = TestLots.Active(startedAt: TestLots.Start.AddHours(5));
        var sooner = TestLots.Active(startedAt: TestLots.Start);

        var ordered = new EndingSoonSort().Apply(new[] { later, sooner }.AsQueryable(), null).ToList();

        Assert.Equal([sooner, later], ordered);
    }

    [Fact]
    public void Relevance_puts_title_matches_first()
    {
        var other = TestLots.Active(startedAt: TestLots.Start, title: "Гітара акустична");
        var match = TestLots.Active(startedAt: TestLots.Start.AddHours(3), title: "Фотоапарат Зеніт");

        var ordered = new RelevanceSort().Apply(new[] { other, match }.AsQueryable(), "зеніт").ToList();

        Assert.Equal(match, ordered[0]);
    }

    [Fact]
    public void Most_bids_orders_by_bid_count()
    {
        var quiet = TestLots.Active();
        var popular = TestLots.Active();
        popular.ApplyBid(5, 100, isNewBidder: true);
        popular.ApplyBid(6, 110, isNewBidder: true);

        var ordered = new MostBidsSort().Apply(new[] { quiet, popular }.AsQueryable(), null).ToList();

        Assert.Equal(popular, ordered[0]);
    }

    [Fact]
    public void Ends_within_hour_keeps_only_lots_ending_in_the_next_hour()
    {
        var now = TestLots.Start.AddDays(1).AddMinutes(-30);
        var endingSoon = TestLots.Active(startedAt: TestLots.Start);
        var endingTomorrow = TestLots.Active(startedAt: TestLots.Start.AddDays(1));

        var filtered = Resolver.ResolveFilter(EndsWithin.Hour)
            .Apply(new[] { endingSoon, endingTomorrow }.AsQueryable(), now)
            .ToList();

        Assert.Equal([endingSoon], filtered);
    }

    [Fact]
    public void Any_filter_keeps_everything()
    {
        var lots = new[] { TestLots.Active(), TestLots.NewDraft() };

        var filtered = Resolver.ResolveFilter(null).Apply(lots.AsQueryable(), TestLots.Start).ToList();

        Assert.Equal(lots, filtered);
    }
}
