// Session rows owned by the Users module; the Platform authentication handler reads and renews them.
using Auction.Core.Common.Enums;

namespace Auction.Core.Users.Contracts;

public sealed record SessionRecord(
    long SessionId,
    long UserId,
    UserRole Role,
    UserStatus UserStatus,
    bool IsEmailConfirmed,
    DateTimeOffset LastSeenAt,
    DateTimeOffset ExpiresAt);

public interface ISessionStore
{
    Task<SessionRecord?> FindByTokenHashAsync(byte[] tokenHash, CancellationToken ct);

    Task CreateAsync(long userId, byte[] tokenHash, DateTimeOffset now, DateTimeOffset expiresAt, string? userAgent, CancellationToken ct);

    Task ExtendAsync(long sessionId, DateTimeOffset lastSeenAt, DateTimeOffset expiresAt, CancellationToken ct);

    Task DeleteAsync(byte[] tokenHash, CancellationToken ct);
}
