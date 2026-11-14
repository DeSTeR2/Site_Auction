// Allowed and forbidden lot status transitions.
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.Core.Lots.Domain;

namespace Auction.UnitTests.Lots;

public class LotStateMachineTests
{
    [Theory]
    [InlineData(LotStatus.Draft, LotTrigger.Publish, LotStatus.Active)]
    [InlineData(LotStatus.Draft, LotTrigger.SubmitForReview, LotStatus.PendingReview)]
    [InlineData(LotStatus.PendingReview, LotTrigger.Approve, LotStatus.Active)]
    [InlineData(LotStatus.PendingReview, LotTrigger.Reject, LotStatus.Draft)]
    [InlineData(LotStatus.PendingReview, LotTrigger.CancelReview, LotStatus.Draft)]
    [InlineData(LotStatus.Active, LotTrigger.Withdraw, LotStatus.Withdrawn)]
    public void Allowed_transition_returns_next_status(LotStatus current, LotTrigger trigger, LotStatus expected)
    {
        var next = LotStateMachine.Next(current, trigger);

        Assert.True(next.IsSuccess);
        Assert.Equal(expected, next.Value);
    }

    [Theory]
    [InlineData(LotStatus.Active, LotTrigger.Publish)]
    [InlineData(LotStatus.Sold, LotTrigger.Withdraw)]
    [InlineData(LotStatus.Unsold, LotTrigger.Publish)]
    [InlineData(LotStatus.Withdrawn, LotTrigger.Publish)]
    [InlineData(LotStatus.Draft, LotTrigger.Approve)]
    [InlineData(LotStatus.Draft, LotTrigger.Withdraw)]
    public void Forbidden_transition_returns_invalid_transition_error(LotStatus current, LotTrigger trigger)
    {
        var next = LotStateMachine.Next(current, trigger);

        Assert.False(next.IsSuccess);
        Assert.Equal(AppErrors.Lot.InvalidTransition, next.Error);
    }
}
