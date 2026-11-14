// Why a bid attempt was rejected; texts and codes come from BidRejectionFactory.
namespace Auction.Core.Bidding.Contracts;

public enum BidRejectionReason
{
    InvalidAmount,
    TooLow,
    PriceChanged,
    OwnLot,
    AlreadyLeader,
    AuctionEnded,
    LotUnavailable,
    RateLimited,
    EmailNotConfirmed,
    AuthenticationRequired,
    Forbidden
}
