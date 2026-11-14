// Actions recorded in the append-only admin action log (project.md §6.10).
namespace Auction.Core.Common.Enums;

public enum AdminActionType
{
    HideLot,
    RestoreLot,
    AcceptComplaint,
    RejectComplaint,
    BlockUser,
    UnblockUser,
    ApproveLink,
    RejectLink,
    RevokeLink
}
