// Server-side session row; only the SHA-256 of the cookie token is stored (design/07-auth-security.md §1).
using Auction.Core.Common.Events;

namespace Auction.Core.Users.Domain;

public sealed class UserSession : Entity
{
    public const int MaxUserAgentLength = 512;

    private UserSession()
    {
        TokenHash = [];
    }

    public long UserId { get; private set; }

    public byte[] TokenHash { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public string? UserAgent { get; private set; }

    public static UserSession Start(long userId, byte[] tokenHash, DateTimeOffset now, DateTimeOffset expiresAt, string? userAgent) => new()
    {
        UserId = userId,
        TokenHash = tokenHash,
        CreatedAt = now,
        LastSeenAt = now,
        ExpiresAt = expiresAt,
        UserAgent = userAgent is { Length: > MaxUserAgentLength } ? userAgent[..MaxUserAgentLength] : userAgent
    };

    public bool IsExpired(DateTimeOffset now) => ExpiresAt <= now;

    public void Extend(DateTimeOffset lastSeenAt, DateTimeOffset expiresAt)
    {
        LastSeenAt = lastSeenAt;
        ExpiresAt = expiresAt;
    }
}
