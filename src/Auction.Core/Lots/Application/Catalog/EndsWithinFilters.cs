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

public sealed class EndsWithinHourFilter : IEndsWithinFilter
{
    public EndsWithin Key => EndsWithin.Hour;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, DateTimeOffset now)
    {
        var limit = now.AddHours(1);
        return query.Where(l => l.EndsAt != null && l.EndsAt > now && l.EndsAt <= limit);
    }
}

public sealed class EndsWithinDayFilter : IEndsWithinFilter
{
    public EndsWithin Key => EndsWithin.Day;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, DateTimeOffset now)
    {
        var limit = now.AddHours(24);
        return query.Where(l => l.EndsAt != null && l.EndsAt > now && l.EndsAt <= limit);
    }
}

public sealed class EndsWithinThreeDaysFilter : IEndsWithinFilter
{
    public EndsWithin Key => EndsWithin.ThreeDays;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, DateTimeOffset now)
    {
        var limit = now.AddDays(3);
        return query.Where(l => l.EndsAt != null && l.EndsAt > now && l.EndsAt <= limit);
    }
}

public sealed class EndsWithinAnyFilter : IEndsWithinFilter
{
    public EndsWithin Key => EndsWithin.Any;

    public IQueryable<Lot> Apply(IQueryable<Lot> query, DateTimeOffset now) => query;
}
