// Accepted bid: immutable once stored, never edited or deleted (project.md §6.5).
using Auction.Core.Common.Events;

namespace Auction.Core.Bidding.Domain;

public sealed class Bid : Entity
{
    private Bid()
    {
    }

    public long LotId { get; private set; }

    public long BidderId { get; private set; }

    public long Amount { get; private set; }

    public DateTimeOffset PlacedAt { get; private set; }

    public static Bid Place(long lotId, long bidderId, long amount, DateTimeOffset placedAt) => new()
    {
        LotId = lotId,
        BidderId = bidderId,
        Amount = amount,
        PlacedAt = placedAt
    };

    public bool IsAbove(Bid other) => Amount > other.Amount;
}
