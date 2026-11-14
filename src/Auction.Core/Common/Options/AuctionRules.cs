// Business constants from project.md §16 bound to the "Auction" configuration section.
using System.ComponentModel.DataAnnotations;

namespace Auction.Core.Common.Options;

public sealed class AuctionRules
{
    public const string SectionName = "Auction";

    [Range(1, 1000)]
    public int MaxActiveLotsPerUser { get; init; } = 20;

    [Range(1, 1000)]
    public int MaxDraftsPerUser { get; init; } = 20;

    [Range(1, 10_000)]
    public int MaxFavoritesPerUser { get; init; } = 200;

    [Range(1, long.MaxValue)]
    public long MaxBidAmount { get; init; } = 100_000_000;

    [MinLength(1)]
    public int[] AllowedDurationsDays { get; init; } = [1, 3, 5, 7, 10, 14];

    public bool AllowMinuteDurations { get; init; }

    [Range(1, 100)]
    public int CatalogPageSize { get; init; } = 24;
}
