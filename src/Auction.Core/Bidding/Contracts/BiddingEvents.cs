// Domain events published by the Bidding module (design/02-modules.md §5).
using Auction.Core.Common.Events;

namespace Auction.Core.Bidding.Contracts;

public sealed record BidPlaced(
    long LotId,
    long BidId,
    long BidderId,
    long SellerId,
    long Amount,
    long? PreviousLeaderId,
    DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);

public sealed record BidRejected(long LotId, long? UserId, BidRejectionReason Reason, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);

public sealed record LotClosed(
    long LotId,
    long SellerId,
    long? WinnerId,
    long? FinalPrice,
    DateTimeOffset OccurredAt) : DomainEvent(OccurredAt)
{
    public bool IsSold => WinnerId is not null;
}
