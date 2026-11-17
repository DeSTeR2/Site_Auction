// Append-only admin action log written in the same transaction as the action.
using Auction.Core.Common.Enums;

namespace Auction.Core.Platform.Contracts;

public interface IAdminActionLog
{
    Task RecordAsync(AdminActionType action, string targetType, long targetId, string? reason, CancellationToken ct);
}
