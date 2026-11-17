// One-time token for email confirmation or password reset; only its hash is stored.
using Auction.Core.Common.Enums;
using Auction.Core.Common.Events;

namespace Auction.Core.Users.Domain;

public sealed class UserToken : Entity
{
    private UserToken()
    {
        TokenHash = [];
    }

    public long UserId { get; private set; }

    public TokenPurpose Purpose { get; private set; }

    public byte[] TokenHash { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public static UserToken Issue(long userId, TokenPurpose purpose, byte[] tokenHash, DateTimeOffset now, TimeSpan lifetime) => new()
    {
        UserId = userId,
        Purpose = purpose,
        TokenHash = tokenHash,
        CreatedAt = now,
        ExpiresAt = now + lifetime
    };

    public bool IsUsable(DateTimeOffset now) => UsedAt is null && ExpiresAt >= now;

    public void MarkUsed(DateTimeOffset now) => UsedAt = now;
}
