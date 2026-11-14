// Bid rule chain: phase filtering, fixed order and stop on first rejection.
using Auction.Core.Bidding.Application.Rules;
using Auction.Core.Bidding.Contracts;

namespace Auction.UnitTests.Bidding;

public class BidRuleChainTests
{
    private static readonly BidContext Context = new(UserId: 1, IsAdmin: false, IsEmailConfirmed: true, Amount: 100, SeenMinimumBid: 100);

    [Fact]
    public void Rules_run_in_order_within_phase_only()
    {
        var calls = new List<string>();
        var chain = new BidRuleChain(
        [
            new RecordingRule("second", BidPhase.BeforeLock, 20, calls),
            new RecordingRule("under-lock", BidPhase.UnderLock, 1, calls),
            new RecordingRule("first", BidPhase.BeforeLock, 10, calls),
        ]);

        var result = chain.Run(BidPhase.BeforeLock, Context);

        Assert.True(result.IsPass);
        Assert.Equal(["first", "second"], calls);
    }

    [Fact]
    public void Chain_stops_at_first_rejection()
    {
        var calls = new List<string>();
        var chain = new BidRuleChain(
        [
            new RecordingRule("own-lot", BidPhase.UnderLock, 10, calls, BidRejectionReason.OwnLot),
            new RecordingRule("leader", BidPhase.UnderLock, 20, calls, BidRejectionReason.AlreadyLeader),
        ]);

        var result = chain.Run(BidPhase.UnderLock, Context);

        Assert.Equal(BidRejectionReason.OwnLot, result.Reason);
        Assert.Equal(["own-lot"], calls);
    }

    [Fact]
    public void Check_maps_rejection_to_app_error()
    {
        var chain = new BidRuleChain([new RecordingRule("rate", BidPhase.BeforeLock, 1, [], BidRejectionReason.RateLimited)]);

        var result = chain.Check(BidPhase.BeforeLock, Context);

        Assert.Equal("bid.rate_limited", result.Error!.Code);
        Assert.Equal(429, result.Error.Status);
    }

    private sealed class RecordingRule(
        string name,
        BidPhase phase,
        int order,
        List<string> calls,
        BidRejectionReason? rejectWith = null) : IBidRule
    {
        public BidPhase Phase => phase;

        public int Order => order;

        public BidRuleResult Check(BidContext context)
        {
            calls.Add(name);
            return rejectWith is null ? BidRuleResult.Pass : BidRuleResult.Reject(rejectWith.Value);
        }
    }
}
