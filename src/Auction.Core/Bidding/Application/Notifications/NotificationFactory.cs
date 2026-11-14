// Builds a notification row from the template registered for its type (design/11-patterns.md §1.1).
using Auction.Core.Bidding.Contracts;
using Auction.Core.Bidding.Domain;
using Auction.Core.Common.Enums;

namespace Auction.Core.Bidding.Application.Notifications;

public sealed class NotificationFactory(IEnumerable<INotificationTemplate> templates, TimeProvider clock)
{
    private readonly Dictionary<NotificationType, INotificationTemplate> byType = templates.ToDictionary(t => t.Type);

    public Notification Create(NotificationType type, long recipientId, NotificationContext context, bool recipientOptedIn)
    {
        if (!byType.TryGetValue(type, out var template))
        {
            throw new InvalidOperationException($"No notification template is registered for {type}.");
        }

        var emailStatus = template.EmailPolicy switch
        {
            EmailPolicy.Always => EmailStatus.Pending,
            EmailPolicy.IfOptedIn when recipientOptedIn => EmailStatus.Pending,
            _ => EmailStatus.None
        };

        return Notification.New(
            recipientId,
            type,
            context.LotId,
            template.BuildText(context),
            $"/lots/{context.LotId}",
            emailStatus,
            clock.GetUtcNow());
    }
}
