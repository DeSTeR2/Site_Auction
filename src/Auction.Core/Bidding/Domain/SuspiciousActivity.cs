// Entry of the suspicious activity log that administrators review (project.md §9.10).
using System.Text.Json;
using Auction.Core.Common.Enums;
using Auction.Core.Common.Events;

namespace Auction.Core.Bidding.Domain;

public sealed class SuspiciousActivity : Entity
{
    private SuspiciousActivity()
    {
        Details = "{}";
    }

    public long UserId { get; private set; }

    public long? LotId { get; private set; }

    public SuspiciousActivityType Type { get; private set; }

    public string Details { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsReviewed { get; private set; }

    public static SuspiciousActivity Record(
        SuspiciousActivityType type,
        long userId,
        long? lotId,
        IReadOnlyDictionary<string, object?>? details,
        DateTimeOffset now) => new()
    {
        Type = type,
        UserId = userId,
        LotId = lotId,
        Details = JsonSerializer.Serialize(details ?? new Dictionary<string, object?>()),
        CreatedAt = now
    };

    public void MarkReviewed() => IsReviewed = true;
}
