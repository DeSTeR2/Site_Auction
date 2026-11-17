// Review state of a complaint: new until an administrator accepts or rejects it (project.md §6.8).
namespace Auction.Core.Common.Enums;

public enum ComplaintStatus
{
    New,
    Accepted,
    Rejected
}
