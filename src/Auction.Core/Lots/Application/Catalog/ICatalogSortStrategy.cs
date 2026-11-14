// Catalog sort keys and the strategy contract that applies one of them to a lot query.
using Auction.Core.Lots.Domain;

namespace Auction.Core.Lots.Application.Catalog;

public enum CatalogSort
{
    EndingSoon,
    Newest,
    Cheapest,
    Priciest,
    MostBids,
    RecentlyEnded,
    Relevance
}

public interface ICatalogSortStrategy
{
    CatalogSort Key { get; }

    IQueryable<Lot> Apply(IQueryable<Lot> query, string? text);
}
