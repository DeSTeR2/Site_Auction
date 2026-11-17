// "Ends within" catalog filters: 1 hour, 24 hours, 3 days or any time (project.md §9.5).
using Auction.Core.Lots.Domain;

namespace Auction.Core.Lots.Application.Catalog;

public enum EndsWithin
{
    Hour,
    Day,
    ThreeDays,
    Any
}

public interface IEndsWithinFilter
{
    EndsWithin Key { get; }

    IQueryable<Lot> Apply(IQueryable<Lot> query, DateTimeOffset now);
}

public abstract class EndsWithinPeriodFilter(EndsWithin key, TimeSpan period) : IEndsWithinFilter
{
    public EndsWithin Key => key;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, DateTimeOffset now)
    {
        var limit = now + period;
        return query.Where(l => l.EndsAt != null && l.EndsAt > now && l.EndsAt <= limit);
    }
}

public sealed class EndsWithinHourFilter() : EndsWithinPeriodFilter(EndsWithin.Hour, TimeSpan.FromHours(1));

public sealed class EndsWithinDayFilter() : EndsWithinPeriodFilter(EndsWithin.Day, TimeSpan.FromHours(24));

public sealed class EndsWithinThreeDaysFilter() : EndsWithinPeriodFilter(EndsWithin.ThreeDays, TimeSpan.FromDays(3));

public sealed class EndsWithinAnyFilter : IEndsWithinFilter
{
    public EndsWithin Key => EndsWithin.Any;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, DateTimeOffset now) => query;
}
