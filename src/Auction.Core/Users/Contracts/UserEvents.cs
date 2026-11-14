// Domain events published by the Users module and handled by other modules.
using Auction.Core.Common.Events;

namespace Auction.Core.Users.Contracts;

public sealed record UserBlocked(long UserId, long AdminId, string Reason, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);
