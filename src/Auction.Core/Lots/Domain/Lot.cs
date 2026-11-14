// Lot aggregate: status changes go through LotStateMachine, bidding columns only through ApplyBid and Close.
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.Core.Common.Events;
using Auction.Core.Lots.Contracts;

namespace Auction.Core.Lots.Domain;

public sealed class Lot : Entity, ILockedLot
{
    private Lot()
    {
        Title = string.Empty;
        Description = string.Empty;
        DonationUrl = string.Empty;
    }

    internal Lot(long sellerId, LotDraftInput input, DateTimeOffset createdAt, long? sourceLotId = null)
    {
        SellerId = sellerId;
        CategoryId = input.CategoryId;
        Title = input.Title;
        Description = input.Description;
        Condition = input.Condition;
        StartPrice = input.StartPrice;
        BidStep = input.BidStep;
        DurationMinutes = input.DurationMinutes;
        DonationUrl = input.DonationUrl;
        SourceLotId = sourceLotId;
        CreatedAt = createdAt;
        Status = LotStatus.Draft;
        CurrentPrice = input.StartPrice;
    }

    public long SellerId { get; private set; }

    public short CategoryId { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public LotCondition Condition { get; private set; }

    public long StartPrice { get; private set; }

    public long BidStep { get; private set; }

    public int DurationMinutes { get; private set; }

    public LotStatus Status { get; private set; }

    public bool IsHidden { get; private set; }

    public string? HiddenReason { get; private set; }

    public DateTimeOffset? HiddenAt { get; private set; }

    public string DonationUrl { get; private set; }

    public long? DonationLinkId { get; private set; }

    public string? ReviewRejectionReason { get; private set; }

    public long? SourceLotId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? EndsAt { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public long CurrentPrice { get; private set; }

    public long? LeaderId { get; private set; }

    public int BidCount { get; private set; }

    public int BidderCount { get; private set; }

    public long? WinnerId { get; private set; }

    public long? FinalPrice { get; private set; }

    public int ViewCount { get; private set; }

    public bool HasBids => BidCount > 0;

    public bool IsCompleted => Status is LotStatus.Sold or LotStatus.Unsold;

    public Result<Unit> Publish(DateTimeOffset now)
    {
        var moved = Move(LotTrigger.Publish);
        if (!moved.IsSuccess)
        {
            return moved;
        }

        StartBidding(now);
        Raise(new LotPublished(Id, SellerId, EndsAt!.Value, now));
        return moved;
    }

    public Result<Unit> SubmitForReview(DateTimeOffset now)
    {
        var moved = Move(LotTrigger.SubmitForReview);
        if (!moved.IsSuccess)
        {
            return moved;
        }

        SubmittedAt = now;
        ReviewRejectionReason = null;
        Raise(new LotSubmittedForReview(Id, SellerId, now));
        return moved;
    }

    public Result<Unit> Approve(DateTimeOffset now)
    {
        var moved = Move(LotTrigger.Approve);
        if (!moved.IsSuccess)
        {
            return moved;
        }

        StartBidding(now);
        Raise(new LotApproved(Id, SellerId, EndsAt!.Value, now));
        return moved;
    }

    public Result<Unit> Reject(string reason, DateTimeOffset now)
    {
        var moved = Move(LotTrigger.Reject);
        if (!moved.IsSuccess)
        {
            return moved;
        }

        ReviewRejectionReason = reason;
        Raise(new LotRejected(Id, SellerId, reason, now));
        return moved;
    }

    public Result<Unit> CancelReview() => Move(LotTrigger.CancelReview);

    public Result<Unit> Withdraw(DateTimeOffset now)
    {
        if (HasBids)
        {
            return AppErrors.Lot.HasBids;
        }

        var moved = Move(LotTrigger.Withdraw);
        if (!moved.IsSuccess)
        {
            return moved;
        }

        Raise(new LotWithdrawn(Id, SellerId, now));
        return moved;
    }

    public Result<Unit> Hide(string reason, DateTimeOffset now)
    {
        if (IsHidden || Status is LotStatus.Draft or LotStatus.PendingReview)
        {
            return AppErrors.Lot.InvalidTransition;
        }

        IsHidden = true;
        HiddenReason = reason;
        HiddenAt = now;
        Raise(new LotHidden(Id, SellerId, reason, now));
        return Result.Success();
    }

    public Result<Unit> Restore(DateTimeOffset now)
    {
        if (!IsHidden || EndsAt is null || now >= EndsAt)
        {
            return AppErrors.Lot.InvalidTransition;
        }

        IsHidden = false;
        HiddenReason = null;
        HiddenAt = null;
        Raise(new LotRestored(Id, SellerId, now));
        return Result.Success();
    }

    public void ApplyBid(long bidderId, long amount, bool isNewBidder)
    {
        if (Status != LotStatus.Active || IsHidden)
        {
            throw new InvalidOperationException($"Lot {Id} does not accept bids in status {Status}.");
        }

        CurrentPrice = amount;
        LeaderId = bidderId;
        BidCount++;
        if (isNewBidder)
        {
            BidderCount++;
        }
    }

    public bool Close(DateTimeOffset now)
    {
        if (Status != LotStatus.Active || EndsAt is null || now < EndsAt)
        {
            return false;
        }

        if (HasBids && !IsHidden)
        {
            Status = LotStatus.Sold;
            WinnerId = LeaderId;
            FinalPrice = CurrentPrice;
        }
        else
        {
            Status = LotStatus.Unsold;
        }

        ClosedAt = now;
        return true;
    }

    internal void ForceDemoState(LotStatus status, DateTimeOffset? startedAt)
    {
        Status = status;
        if (startedAt is not null)
        {
            StartBidding(startedAt.Value);
        }
    }

    private void StartBidding(DateTimeOffset now)
    {
        StartedAt = now;
        EndsAt = now.AddMinutes(DurationMinutes);
    }

    private Result<Unit> Move(LotTrigger trigger)
    {
        var next = LotStateMachine.Next(Status, trigger);
        if (!next.IsSuccess)
        {
            return next.Error!;
        }

        Status = next.Value;
        return Result.Success();
    }
}
