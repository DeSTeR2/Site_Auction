// Read-only lot snapshot for Bidding and Users (project.md §13.1).
using Auction.Core.Common.Enums;

namespace Auction.Core.Lots.Contracts;

public sealed record LotSnapshot(
    long Id,
    long SellerId,
    string Title,
    LotStatus Status,
    bool IsHidden,
    long StartPrice,
    long BidStep,
    long CurrentPrice,
    long? LeaderId,
    int BidCount,
    int BidderCount,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndsAt);

public interface ILotReader
{
    Task<LotSnapshot?> GetSnapshotAsync(long lotId, CancellationToken ct);
}
