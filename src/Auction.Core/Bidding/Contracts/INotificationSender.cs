// The single way any module sends a user notification (project.md §13.1).
using Auction.Core.Common.Enums;

namespace Auction.Core.Bidding.Contracts;

public sealed record NotificationContext(
    long LotId,
    string LotTitle,
    long? Amount = null,
    string? Reason = null,
    string? DonationUrl = null);

public interface INotificationSender
{
    Task SendAsync(NotificationType type, long recipientId, NotificationContext context, CancellationToken ct);
}
