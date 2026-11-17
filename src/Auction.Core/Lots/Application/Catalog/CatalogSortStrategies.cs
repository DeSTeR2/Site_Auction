// Catalog sort strategies from design/11-patterns.md §2; completed lots sort by final price.
using Auction.Core.Lots.Domain;

namespace Auction.Core.Lots.Application.Catalog;

public sealed class EndingSoonSort : ICatalogSortStrategy
{
    public CatalogSort Key => CatalogSort.EndingSoon;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, string? text) =>
        query.OrderBy(l => l.EndsAt).ThenBy(l => l.Id);
}

public sealed class NewestSort : ICatalogSortStrategy
{
    public CatalogSort Key => CatalogSort.Newest;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, string? text) =>
        query.OrderByDescending(l => l.StartedAt).ThenByDescending(l => l.Id);
}

public sealed class CheapestSort : ICatalogSortStrategy
{
    public CatalogSort Key => CatalogSort.Cheapest;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, string? text) =>
        query.OrderBy(l => l.FinalPrice ?? l.CurrentPrice).ThenBy(l => l.Id);
}

public sealed class PriciestSort : ICatalogSortStrategy
{
    public CatalogSort Key => CatalogSort.Priciest;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, string? text) =>
        query.OrderByDescending(l => l.FinalPrice ?? l.CurrentPrice).ThenBy(l => l.Id);
}

public sealed class MostBidsSort : ICatalogSortStrategy
{
    public CatalogSort Key => CatalogSort.MostBids;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, string? text) =>
        query.OrderByDescending(l => l.BidCount).ThenBy(l => l.EndsAt);
}

public sealed class RecentlyEndedSort : ICatalogSortStrategy
{
    public CatalogSort Key => CatalogSort.RecentlyEnded;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, string? text) =>
        query.OrderByDescending(l => l.ClosedAt).ThenByDescending(l => l.Id);
}

public sealed class RelevanceSort : ICatalogSortStrategy
{
    public CatalogSort Key => CatalogSort.Relevance;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return query.OrderBy(l => l.EndsAt).ThenBy(l => l.Id);
        }

        var term = text.Trim().ToLower();
        return query
            .OrderByDescending(l => l.Title.ToLower().Contains(term))
            .ThenBy(l => l.EndsAt)
            .ThenBy(l => l.Id);
    }
}
