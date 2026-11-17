// Suspicious activity journal: entries are saved at once so they survive a rejected bid or login (project.md §9.10).
using System.Text.Json;
using Auction.Core.Bidding.Contracts;
using Auction.Core.Bidding.Domain;
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.Core.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auction.Core.Bidding.Application;

public sealed class SuspiciousActivityLog(IAuctionDbContext db, TimeProvider clock) : ISuspiciousActivityLog
{
    public async Task RecordAsync(
        SuspiciousActivityType type,
        long userId,
        long? lotId,
        IReadOnlyDictionary<string, object?> details,
        CancellationToken ct)
    {
        var activities = db.Set<SuspiciousActivity>();
        var activity = SuspiciousActivity.Record(type, userId, lotId, details, clock.GetUtcNow());
        activities.Add(activity);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            activities.Entry(activity).State = EntityState.Detached;
        }
    }

    public async Task<PagedList<SuspiciousActivityEntry>> ListAsync(
        SuspiciousActivityType? type,
        PageRequest page,
        CancellationToken ct)
    {
        var query = db.Set<SuspiciousActivity>().AsNoTracking();
        if (type is { } wanted)
        {
            query = query.Where(activity => activity.Type == wanted);
        }

        var size = Math.Min(page.PageSize, 50);
        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(activity => activity.CreatedAt)
            .Skip((page.Page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedList<SuspiciousActivityEntry>(rows.Select(ToEntry).ToList(), page.Page, size, total);
    }

    private static SuspiciousActivityEntry ToEntry(SuspiciousActivity activity) => new(
        activity.Id,
        activity.Type,
        activity.UserId,
        activity.LotId,
        JsonSerializer.Deserialize<Dictionary<string, object?>>(activity.Details) ?? [],
        activity.CreatedAt,
        activity.IsReviewed);
}
