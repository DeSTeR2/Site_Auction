// Reasons a user can give when reporting a lot (project.md §6.8).
namespace Auction.Core.Common.Enums;

public enum ComplaintReason
{
    ProhibitedItem,
    FraudOrCounterfeit,
    FalseDescription,
    OffensiveContent,
    FakeBids,
    Other
}
