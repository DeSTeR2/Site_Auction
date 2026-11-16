// Stored product analytics event (project.md §12) with its payload kept as JSON.
using System.Text.Json;
using Auction.Core.Common.Events;
using Auction.Core.Platform.Contracts;

namespace Auction.Core.Platform.Domain;

public sealed class AnalyticsRecord : Entity
{
    private AnalyticsRecord()
    {
        Type = string.Empty;
        Payload = "{}";
    }

    public string Type { get; private set; }

    public long? UserId { get; private set; }

    public long? LotId { get; private set; }

    public string? VisitorKey { get; private set; }

    public string Payload { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static AnalyticsRecord From(AnalyticsEvent analyticsEvent, string? visitorKey, DateTimeOffset now) => new()
    {
        Type = analyticsEvent.Type,
        UserId = analyticsEvent.UserId,
        LotId = analyticsEvent.LotId,
        VisitorKey = visitorKey,
        Payload = JsonSerializer.Serialize(analyticsEvent.Payload ?? new Dictionary<string, object?>()),
        CreatedAt = now
    };
}
