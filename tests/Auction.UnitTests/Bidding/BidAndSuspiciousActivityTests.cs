// Bid and suspicious activity rows: fields set on creation and the review flag.
using Auction.Core.Bidding.Domain;
using Auction.Core.Common.Enums;

namespace Auction.UnitTests.Bidding;

public class BidAndSuspiciousActivityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Placed_bid_keeps_lot_bidder_amount_and_time()
    {
        var bid = Bid.Place(lotId: 10, bidderId: 3, amount: 650, Now);

        Assert.Equal(10, bid.LotId);
        Assert.Equal(3, bid.BidderId);
        Assert.Equal(650, bid.Amount);
        Assert.Equal(Now, bid.PlacedAt);
    }

    [Fact]
    public void Record_serializes_details_as_json()
    {
        var activity = SuspiciousActivity.Record(
            SuspiciousActivityType.RateLimit,
            userId: 3,
            lotId: 10,
            new Dictionary<string, object?> { ["attempts"] = 4 },
            Now);

        Assert.Equal("""{"attempts":4}""", activity.Details);
        Assert.False(activity.IsReviewed);
    }

    [Fact]
    public void Record_without_details_stores_an_empty_json_object()
    {
        var activity = SuspiciousActivity.Record(SuspiciousActivityType.LoginFailures, userId: 3, lotId: null, details: null, Now);

        Assert.Equal("{}", activity.Details);
        Assert.Null(activity.LotId);
    }

    [Fact]
    public void MarkReviewed_sets_the_flag()
    {
        var activity = SuspiciousActivity.Record(SuspiciousActivityType.OwnLotBid, userId: 3, lotId: 10, details: null, Now);

        activity.MarkReviewed();

        Assert.True(activity.IsReviewed);
    }
}
