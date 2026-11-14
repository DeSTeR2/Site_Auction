// Text and email policy of a single notification type (project.md §9.7).
using Auction.Core.Bidding.Contracts;
using Auction.Core.Common.Enums;

namespace Auction.Core.Bidding.Application.Notifications;

public enum EmailPolicy
{
    Never,
    IfOptedIn,
    Always
}

public interface INotificationTemplate
{
    NotificationType Type { get; }

    EmailPolicy EmailPolicy { get; }

    string BuildText(NotificationContext context);
}
