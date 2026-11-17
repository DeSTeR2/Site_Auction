// Read-only view of users for Lots and Bidding (project.md §13.1).
using Auction.Core.Common.Enums;

namespace Auction.Core.Users.Contracts;

public sealed record UserSummary(
    long Id,
    string Nickname,
    UserStatus Status,
    bool IsEmailConfirmed,
    DateTimeOffset RegisteredAt);

public sealed record UserContacts(long Id, string Email, string? Phone);

public interface IUserReader
{
    Task<UserSummary?> GetSummaryAsync(long userId, CancellationToken ct);

    Task<UserContacts?> GetContactsAsync(long userId, CancellationToken ct);
}
