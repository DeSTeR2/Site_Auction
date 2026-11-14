// Bid rate limits from design/04-bidding-concurrency.md §5, bound to "Bidding:RateLimits".
using System.ComponentModel.DataAnnotations;

namespace Auction.Core.Bidding.Application;

public sealed class RateLimitOptions
{
    public const string SectionName = "Bidding:RateLimits";

    [Range(1, 1000)]
    public int PerUserPerMinute { get; init; } = 10;

    [Range(1, 1000)]
    public int PerLotPerMinute { get; init; } = 2;
}
