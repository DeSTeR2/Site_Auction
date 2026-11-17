// One bid rule from project.md §8.1–8.3; rules run in phase A (no lock) or phase B (under FOR UPDATE).
using Auction.Core.Bidding.Contracts;
using Auction.Core.Bidding.Domain;
using Auction.Core.Lots.Contracts;

namespace Auction.Core.Bidding.Application.Rules;

public enum BidPhase
{
    BeforeLock,
    UnderLock
}

public sealed record BidContext(
    long? UserId,
    bool IsAdmin,
    bool IsEmailConfirmed,
    long Amount,
    long SeenMinimumBid)
{
    public ILockedLot? Lot { get; init; }

    public DateTimeOffset? Now { get; init; }
}

public sealed record BidRuleResult(BidRejectionReason? Reason, BidRejectionData? Data)
{
    public static readonly BidRuleResult Pass = new(null, null);

    public bool IsPass => Reason is null;

    public static BidRuleResult Reject(BidRejectionReason reason, BidRejectionData? data = null) => new(reason, data);
}

public interface IBidRule
{
    BidPhase Phase { get; }

    int Order { get; }

    BidRuleResult Check(BidContext context);
}
