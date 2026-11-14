// Stored lot status; hidden is a separate flag on top of it (design/03-data-model.md).
namespace Auction.Core.Common.Enums;

public enum LotStatus
{
    Draft,
    PendingReview,
    Active,
    Sold,
    Unsold,
    Withdrawn
}
