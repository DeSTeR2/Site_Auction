// A lot a user follows; the ending reminder is sent once and remembered here (project.md §9.6).
namespace Auction.Core.Lots.Domain;

public sealed class Favorite
{
    private Favorite()
    {
    }

    public long UserId { get; private set; }

    public long LotId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ReminderSentAt { get; private set; }

    public static Favorite Add(long userId, long lotId, DateTimeOffset now) => new()
    {
        UserId = userId,
        LotId = lotId,
        CreatedAt = now
    };

    public void MarkReminderSent(DateTimeOffset now) => ReminderSentAt = now;
}
