// Lot aggregate behaviour: publishing, withdrawing, hiding and closing (project.md §7.3).
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.Core.Lots.Contracts;

namespace Auction.UnitTests.Lots;

public class LotTests
{
    [Fact]
    public void Publish_starts_bidding_and_raises_event()
    {
        var lot = TestLots.NewDraft();

        var result = lot.Publish(TestLots.Start);

        Assert.True(result.IsSuccess);
        Assert.Equal(LotStatus.Active, lot.Status);
        Assert.Equal(TestLots.Start, lot.StartedAt);
        Assert.Equal(TestLots.Start.AddDays(1), lot.EndsAt);
        Assert.IsType<LotPublished>(Assert.Single(lot.DomainEvents));
    }

    [Fact]
    public void Withdraw_is_rejected_after_first_bid()
    {
        var lot = TestLots.Active();
        lot.ApplyBid(7, 100, isNewBidder: true);

        var result = lot.Withdraw(TestLots.Start.AddHours(1));

        Assert.Equal(AppErrors.Lot.HasBids, result.Error);
        Assert.Equal(LotStatus.Active, lot.Status);
    }

    [Fact]
    public void Apply_bid_updates_bidding_columns()
    {
        var lot = TestLots.Active();

        lot.ApplyBid(7, 100, isNewBidder: true);
        lot.ApplyBid(8, 110, isNewBidder: true);
        lot.ApplyBid(7, 120, isNewBidder: false);

        Assert.Equal(120, lot.CurrentPrice);
        Assert.Equal(7, lot.LeaderId);
        Assert.Equal(3, lot.BidCount);
        Assert.Equal(2, lot.BidderCount);
    }

    [Fact]
    public void Close_with_bids_sells_to_leader()
    {
        var lot = TestLots.Active();
        lot.ApplyBid(7, 150, isNewBidder: true);

        var closed = lot.Close(lot.EndsAt!.Value);

        Assert.True(closed);
        Assert.Equal(LotStatus.Sold, lot.Status);
        Assert.Equal(7, lot.WinnerId);
        Assert.Equal(150, lot.FinalPrice);
    }

    [Fact]
    public void Close_without_bids_marks_unsold()
    {
        var lot = TestLots.Active();

        lot.Close(lot.EndsAt!.Value);

        Assert.Equal(LotStatus.Unsold, lot.Status);
        Assert.Null(lot.WinnerId);
    }

    [Fact]
    public void Close_of_hidden_lot_with_bids_marks_unsold()
    {
        var lot = TestLots.Active();
        lot.ApplyBid(7, 150, isNewBidder: true);
        lot.Hide("Шахрайство", TestLots.Start.AddHours(1));

        lot.Close(lot.EndsAt!.Value);

        Assert.Equal(LotStatus.Unsold, lot.Status);
        Assert.Null(lot.WinnerId);
    }

    [Fact]
    public void Close_before_end_time_does_nothing_and_second_close_is_ignored()
    {
        var lot = TestLots.Active();

        Assert.False(lot.Close(lot.EndsAt!.Value.AddSeconds(-1)));
        Assert.True(lot.Close(lot.EndsAt!.Value));
        Assert.False(lot.Close(lot.EndsAt!.Value.AddMinutes(1)));
    }

    [Fact]
    public void Draft_cannot_be_hidden()
    {
        var lot = TestLots.NewDraft();

        var result = lot.Hide("Причина", TestLots.Start);

        Assert.Equal(AppErrors.Lot.InvalidTransition, result.Error);
    }

    [Fact]
    public void Restore_is_rejected_after_end_time()
    {
        var lot = TestLots.Active();
        lot.Hide("Причина", TestLots.Start.AddHours(1));

        var result = lot.Restore(lot.EndsAt!.Value);

        Assert.Equal(AppErrors.Lot.InvalidTransition, result.Error);
        Assert.True(lot.IsHidden);
    }
}
