// Lot row held under SELECT ... FOR UPDATE; the only way Bidding changes the bidding columns.
using Auction.Core.Common.Enums;

namespace Auction.Core.Lots.Contracts;

public interface ILockedLot
{
    long Id { get; }

    long SellerId { get; }

    LotStatus Status { get; }

    bool IsHidden { get; }

    long StartPrice { get; }

    long BidStep { get; }

    long CurrentPrice { get; }

    long? LeaderId { get; }

    int BidCount { get; }

    int BidderCount { get; }

    DateTimeOffset? EndsAt { get; }

    long? WinnerId { get; }

    long? FinalPrice { get; }

    void ApplyBid(long bidderId, long amount, bool isNewBidder);

    bool Close(DateTimeOffset now);
}
