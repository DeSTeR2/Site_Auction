// Bid rejection codes, statuses and texts from project.md §8.8.
using Auction.Core.Bidding.Contracts;
using Auction.Core.Bidding.Domain;

namespace Auction.UnitTests.Bidding;

public class BidRejectionFactoryTests
{
    private static readonly BidRejectionData Data = new(MinimumBid: 700, CurrentPrice: 650, MaxBidAmount: 100_000_000);

    [Fact]
    public void Price_changed_shows_current_and_minimum_price()
    {
        var error = BidRejectionFactory.Create(BidRejectionReason.PriceChanged, Data);

        Assert.Equal("bid.price_changed", error.Code);
        Assert.Equal(409, error.Status);
        Assert.Equal("Поки ви ставили, ціна змінилася. Поточна: 650 грн, мінімальна: 700 грн", error.Message);
    }

    [Fact]
    public void Too_low_shows_minimum_bid()
    {
        var error = BidRejectionFactory.Create(BidRejectionReason.TooLow, Data);

        Assert.Equal("bid.too_low", error.Code);
        Assert.Equal(422, error.Status);
        Assert.Equal("Мінімальна ставка: 700 грн", error.Message);
    }

    [Fact]
    public void Every_reason_has_an_error()
    {
        foreach (var reason in Enum.GetValues<BidRejectionReason>())
        {
            var error = BidRejectionFactory.Create(reason, Data);

            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.InRange(error.Status, 400, 499);
        }
    }

    [Fact]
    public void Reasons_with_amounts_require_data()
    {
        Assert.Throws<ArgumentNullException>(() => BidRejectionFactory.Create(BidRejectionReason.TooLow));
    }
}
