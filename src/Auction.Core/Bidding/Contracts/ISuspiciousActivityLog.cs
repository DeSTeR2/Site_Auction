// Suspicious activity journal written by Bidding and Users, read by admin pages (project.md §9.10).
using Auction.Core.Common;
using Auction.Core.Common.Enums;

namespace Auction.Core.Bidding.Contracts;

public sealed record SuspiciousActivityEntry(
    long Id,
    SuspiciousActivityType Type,
    long UserId,
    long? LotId,
    IReadOnlyDictionary<string, object?> Details,
    DateTimeOffset CreatedAt,
    bool IsReviewed);

public interface ISuspiciousActivityLog
{
    Task RecordAsync(
        SuspiciousActivityType type,
        long userId,
        long? lotId,
        IReadOnlyDictionary<string, object?> details,
        CancellationToken ct);

    Task<PagedList<SuspiciousActivityEntry>> ListAsync(SuspiciousActivityType? type, PageRequest page, CancellationToken ct);
}
