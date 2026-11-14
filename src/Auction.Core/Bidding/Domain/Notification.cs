// On-site notification row; email delivery state is tracked on the same row.
using Auction.Core.Common.Enums;
using Auction.Core.Common.Events;

namespace Auction.Core.Bidding.Domain;

public sealed class Notification : Entity
{
    private Notification()
    {
        Text = string.Empty;
        Link = string.Empty;
    }

    public long RecipientId { get; private set; }

    public NotificationType Type { get; private set; }

    public long LotId { get; private set; }

    public string Text { get; private set; }

    public string Link { get; private set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public EmailStatus EmailStatus { get; private set; }

    public int EmailAttempts { get; private set; }

    public DateTimeOffset? EmailNextAttemptAt { get; private set; }

    public DateTimeOffset? EmailSentAt { get; private set; }

    public static Notification New(
        long recipientId,
        NotificationType type,
        long lotId,
        string text,
        string link,
        EmailStatus emailStatus,
        DateTimeOffset createdAt) => new()
    {
        RecipientId = recipientId,
        Type = type,
        LotId = lotId,
        Text = text,
        Link = link,
        CreatedAt = createdAt,
        EmailStatus = emailStatus,
        EmailNextAttemptAt = emailStatus == EmailStatus.Pending ? createdAt : null
    };

    public void MarkRead() => IsRead = true;
}
