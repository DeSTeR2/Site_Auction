// Photo of a lot: ordered, the first position is the main one; both WebP variants live in file storage.
using Auction.Core.Common.Events;

namespace Auction.Core.Lots.Domain;

public sealed class LotPhoto : Entity
{
    private LotPhoto()
    {
        LargeKey = string.Empty;
        ThumbKey = string.Empty;
    }

    public long LotId { get; private set; }

    public short Position { get; private set; }

    public string LargeKey { get; private set; }

    public string ThumbKey { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsMain => Position == 0;

    public static LotPhoto Add(long lotId, short position, string largeKey, string thumbKey, int width, int height, DateTimeOffset now) => new()
    {
        LotId = lotId,
        Position = position,
        LargeKey = largeKey,
        ThumbKey = thumbKey,
        Width = width,
        Height = height,
        CreatedAt = now
    };

    public void MoveTo(short position) => Position = position;
}
