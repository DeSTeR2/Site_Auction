// Append-only record of an administrator action (project.md §6.10); it has no mutating methods.
using Auction.Core.Common.Enums;
using Auction.Core.Common.Events;

namespace Auction.Core.Platform.Domain;

public sealed class AdminAction : Entity
{
    private AdminAction()
    {
        TargetType = string.Empty;
    }

    public long AdminId { get; private set; }

    public AdminActionType Action { get; private set; }

    public string TargetType { get; private set; }

    public long TargetId { get; private set; }

    public string? Reason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static AdminAction Record(long adminId, AdminActionType action, string targetType, long targetId, string? reason, DateTimeOffset now) => new()
    {
        AdminId = adminId,
        Action = action,
        TargetType = targetType,
        TargetId = targetId,
        Reason = reason,
        CreatedAt = now
    };
}
