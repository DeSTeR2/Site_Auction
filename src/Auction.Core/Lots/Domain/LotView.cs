// One unique visitor of a lot on one day; the row insert decides whether the view counter grows (project.md §9.8).
namespace Auction.Core.Lots.Domain;

public sealed class LotView
{
    private LotView()
    {
        VisitorKey = string.Empty;
    }

    public long LotId { get; private set; }

    public string VisitorKey { get; private set; }

    public DateOnly ViewDate { get; private set; }

    public static string UserKey(long userId) => $"u:{userId}";

    public static string AnonymousKey(string cookieId) => $"a:{cookieId}";

    public static LotView Record(long lotId, string visitorKey, DateOnly viewDate) => new()
    {
        LotId = lotId,
        VisitorKey = visitorKey,
        ViewDate = viewDate
    };
}
