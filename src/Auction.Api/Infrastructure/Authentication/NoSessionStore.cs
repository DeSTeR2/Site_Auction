// Default session store until the Users module registers its own: every visitor stays a guest.
using Auction.Core.Users.Contracts;

namespace Auction.Api.Infrastructure.Authentication;

public sealed class NoSessionStore : ISessionStore
{
    public Task<SessionRecord?> FindByTokenHashAsync(byte[] tokenHash, CancellationToken ct) =>
        Task.FromResult<SessionRecord?>(null);

    public Task CreateAsync(long userId, byte[] tokenHash, DateTimeOffset now, DateTimeOffset expiresAt, string? userAgent, CancellationToken ct) =>
        throw new InvalidOperationException("Sessions are stored by the Users module; register its ISessionStore implementation.");

    public Task ExtendAsync(long sessionId, DateTimeOffset lastSeenAt, DateTimeOffset expiresAt, CancellationToken ct) =>
        Task.CompletedTask;

    public Task DeleteAsync(byte[] tokenHash, CancellationToken ct) => Task.CompletedTask;
}
