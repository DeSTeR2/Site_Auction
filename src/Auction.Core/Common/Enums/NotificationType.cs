// Notification types from project.md §9.7 and design/14-charity.md §5.
namespace Auction.Core.Common.Enums;

public enum NotificationType
{
    Outbid,
    YouWon,
    YouLost,
    LotSold,
    LotUnsold,
    NewBidOnYourLot,
    FavoriteEnding,
    LotHidden,
    LotRestored,
    ComplaintReviewed,
    AccountBlocked,
    LotApproved,
    LotRejected
}
