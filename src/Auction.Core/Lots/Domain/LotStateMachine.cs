// Allowed lot status transitions (project.md §7.2, design/14-charity.md X2); closing is handled by Lot.Close.
using Auction.Core.Common;
using Auction.Core.Common.Enums;

namespace Auction.Core.Lots.Domain;

public enum LotTrigger
{
    Publish,
    SubmitForReview,
    Approve,
    Reject,
    CancelReview,
    Withdraw
}

public static class LotStateMachine
{
    private static readonly Dictionary<(LotStatus, LotTrigger), LotStatus> Transitions = new()
    {
        [(LotStatus.Draft, LotTrigger.Publish)] = LotStatus.Active,
        [(LotStatus.Draft, LotTrigger.SubmitForReview)] = LotStatus.PendingReview,
        [(LotStatus.PendingReview, LotTrigger.Approve)] = LotStatus.Active,
        [(LotStatus.PendingReview, LotTrigger.Reject)] = LotStatus.Draft,
        [(LotStatus.PendingReview, LotTrigger.CancelReview)] = LotStatus.Draft,
        [(LotStatus.Active, LotTrigger.Withdraw)] = LotStatus.Withdrawn,
    };

    public static Result<LotStatus> Next(LotStatus current, LotTrigger trigger) =>
        Transitions.TryGetValue((current, trigger), out var next)
            ? next
            : AppErrors.Lot.InvalidTransition;

    public static bool CanFire(LotStatus current, LotTrigger trigger) => Transitions.ContainsKey((current, trigger));
}
