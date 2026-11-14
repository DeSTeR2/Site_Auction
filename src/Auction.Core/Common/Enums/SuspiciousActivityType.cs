// Kinds of records in the suspicious activity log (project.md §9.10).
namespace Auction.Core.Common.Enums;

public enum SuspiciousActivityType
{
    OwnLotBid,
    RateLimit,
    NewAccountBid,
    LoginFailures
}
