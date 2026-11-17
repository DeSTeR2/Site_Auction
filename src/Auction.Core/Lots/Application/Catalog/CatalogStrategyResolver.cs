// Chooses the sort strategy and time filter for a catalog request without a switch in the query.
namespace Auction.Core.Lots.Application.Catalog;

public sealed class CatalogStrategyResolver(
    IEnumerable<ICatalogSortStrategy> sortStrategies,
    IEnumerable<IEndsWithinFilter> endsWithinFilters)
{
    private readonly Dictionary<CatalogSort, ICatalogSortStrategy> sortsByKey = sortStrategies.ToDictionary(s => s.Key);
    private readonly Dictionary<EndsWithin, IEndsWithinFilter> filtersByKey = endsWithinFilters.ToDictionary(f => f.Key);

    public ICatalogSortStrategy ResolveSort(CatalogSort? requested, string? text)
    {
        var key = requested ?? (string.IsNullOrWhiteSpace(text) ? CatalogSort.EndingSoon : CatalogSort.Relevance);
        return sortsByKey[key];
    }

    public IEndsWithinFilter ResolveFilter(EndsWithin? requested) => filtersByKey[requested ?? EndsWithin.Any];
}
