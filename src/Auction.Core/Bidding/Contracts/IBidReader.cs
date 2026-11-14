// Read-only access to bids for user statistics and admin metrics.
namespace Auction.Core.Bidding.Contracts;

public sealed record BidSummary(long BidId, long LotId, long BidderId, long Amount, DateTimeOffset PlacedAt);

public interface IBidReader
{
    Task<IReadOnlyList<BidSummary>> GetByBidderAsync(long bidderId, CancellationToken ct);

    Task<IReadOnlyList<long>> GetBidderIdsAsync(long lotId, CancellationToken ct);

    Task<int> CountPlacedAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
}
