// Review state of a donation link in the global approved list (design/14-charity.md §3).
namespace Auction.Core.Common.Enums;

public enum DonationLinkStatus
{
    Pending,
    Approved,
    Rejected,
    Revoked
}
