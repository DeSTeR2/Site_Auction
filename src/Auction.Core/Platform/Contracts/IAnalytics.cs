// Product analytics events from project.md §12, stored in the same transaction as the action.
namespace Auction.Core.Platform.Contracts;

public sealed record AnalyticsEvent(
    string Type,
    long? UserId = null,
    long? LotId = null,
    IReadOnlyDictionary<string, object?>? Payload = null);

public interface IAnalytics
{
    Task TrackAsync(AnalyticsEvent analyticsEvent, CancellationToken ct);
}
