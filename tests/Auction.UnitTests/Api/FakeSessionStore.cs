// In-memory ISessionStore for API pipeline tests.
using System.Collections.Concurrent;
using Auction.Core.Common.Enums;
using Auction.Core.Common.Security;
using Auction.Core.Users.Contracts;

namespace Auction.UnitTests.Api;

public sealed class FakeSessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, SessionRecord> sessions = new();
    private long nextId;

    public ConcurrentBag<long> ExtendedSessionIds { get; } = [];

    public int Count => sessions.Count;

    public string Add(
        long userId,
        DateTimeOffset lastSeenAt,
        UserRole role = UserRole.User,
        bool emailConfirmed = true,
        UserStatus status = UserStatus.Active,
        DateTimeOffset? expiresAt = null)
    {
        var token = SecureToken.Generate();
        var record = new SessionRecord(
            Interlocked.Increment(ref nextId),
            userId,
            role,
            status,
            emailConfirmed,
            lastSeenAt,
            expiresAt ?? lastSeenAt.AddDays(30));
        sessions[Key(SecureToken.Hash(token)!)] = record;
        return token;
    }

    public Task<SessionRecord?> FindByTokenHashAsync(byte[] tokenHash, CancellationToken ct) =>
        Task.FromResult(sessions.GetValueOrDefault(Key(tokenHash)));

    public Task CreateAsync(long userId, byte[] tokenHash, DateTimeOffset now, DateTimeOffset expiresAt, string? userAgent, CancellationToken ct)
    {
        sessions[Key(tokenHash)] = new SessionRecord(
            Interlocked.Increment(ref nextId), userId, UserRole.User, UserStatus.Active, true, now, expiresAt);
        return Task.CompletedTask;
    }

    public Task ExtendAsync(long sessionId, DateTimeOffset lastSeenAt, DateTimeOffset expiresAt, CancellationToken ct)
    {
        ExtendedSessionIds.Add(sessionId);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(byte[] tokenHash, CancellationToken ct)
    {
        sessions.TryRemove(Key(tokenHash), out _);
        return Task.CompletedTask;
    }

    private static string Key(byte[] tokenHash) => Convert.ToHexString(tokenHash);
}
