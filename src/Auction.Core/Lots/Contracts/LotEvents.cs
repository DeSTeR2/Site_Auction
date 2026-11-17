// Domain events published by the Lots module (design/02-modules.md §5).
using Auction.Core.Common.Events;

namespace Auction.Core.Lots.Contracts;

public sealed record LotPublished(long LotId, long SellerId, DateTimeOffset EndsAt, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);

public sealed record LotSubmittedForReview(long LotId, long SellerId, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);

public sealed record LotApproved(long LotId, long SellerId, DateTimeOffset EndsAt, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);

public sealed record LotRejected(long LotId, long SellerId, string Reason, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);

public sealed record LotHidden(long LotId, long SellerId, string Reason, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);

public sealed record LotRestored(long LotId, long SellerId, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);

public sealed record LotWithdrawn(long LotId, long SellerId, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);

public sealed record DonationReported(long LotId, long WinnerId, long Amount, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);

public sealed record FavoriteToggled(long LotId, long UserId, bool IsFavorite, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);
