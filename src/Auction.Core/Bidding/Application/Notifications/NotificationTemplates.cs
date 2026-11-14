// Templates for every notification type; a new type is a new class and the factory stays unchanged.
using Auction.Core.Bidding.Contracts;
using Auction.Core.Common;
using Auction.Core.Common.Enums;

namespace Auction.Core.Bidding.Application.Notifications;

public sealed class OutbidTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.Outbid;

    public EmailPolicy EmailPolicy => EmailPolicy.IfOptedIn;

    public string BuildText(NotificationContext context) =>
        $"Вашу ставку на «{context.LotTitle}» перебито. Поточна ціна: {Money.Format(context.Amount ?? 0)}";
}

public sealed class YouWonTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.YouWon;

    public EmailPolicy EmailPolicy => EmailPolicy.Always;

    public string BuildText(NotificationContext context) =>
        $"Ви виграли «{context.LotTitle}» за {Money.Format(context.Amount ?? 0)}. Перерахуйте всю суму на збір: {context.DonationUrl}";
}

public sealed class YouLostTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.YouLost;

    public EmailPolicy EmailPolicy => EmailPolicy.Never;

    public string BuildText(NotificationContext context) =>
        $"Торги за «{context.LotTitle}» завершено. Цього разу ви не виграли";
}

public sealed class LotSoldTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.LotSold;

    public EmailPolicy EmailPolicy => EmailPolicy.Always;

    public string BuildText(NotificationContext context) =>
        $"Лот «{context.LotTitle}» продано за {Money.Format(context.Amount ?? 0)}";
}

public sealed class LotUnsoldTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.LotUnsold;

    public EmailPolicy EmailPolicy => EmailPolicy.IfOptedIn;

    public string BuildText(NotificationContext context) =>
        $"Лот «{context.LotTitle}» завершився без ставок";
}

public sealed class NewBidOnYourLotTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.NewBidOnYourLot;

    public EmailPolicy EmailPolicy => EmailPolicy.Never;

    public string BuildText(NotificationContext context) =>
        $"Нова ставка {Money.Format(context.Amount ?? 0)} на ваш лот «{context.LotTitle}»";
}

public sealed class FavoriteEndingTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.FavoriteEnding;

    public EmailPolicy EmailPolicy => EmailPolicy.Never;

    public string BuildText(NotificationContext context) =>
        $"Обраний лот «{context.LotTitle}» завершиться менш ніж за годину";
}

public sealed class LotHiddenTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.LotHidden;

    public EmailPolicy EmailPolicy => EmailPolicy.IfOptedIn;

    public string BuildText(NotificationContext context) =>
        $"Лот «{context.LotTitle}» приховано адміністратором. Причина: {context.Reason}";
}

public sealed class LotRestoredTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.LotRestored;

    public EmailPolicy EmailPolicy => EmailPolicy.Never;

    public string BuildText(NotificationContext context) =>
        $"Лот «{context.LotTitle}» знову доступний";
}

public sealed class ComplaintReviewedTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.ComplaintReviewed;

    public EmailPolicy EmailPolicy => EmailPolicy.Never;

    public string BuildText(NotificationContext context) =>
        $"Вашу скаргу на лот «{context.LotTitle}» розглянуто";
}

public sealed class AccountBlockedTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.AccountBlocked;

    public EmailPolicy EmailPolicy => EmailPolicy.Always;

    public string BuildText(NotificationContext context) =>
        $"Ваш обліковий запис заблоковано. Причина: {context.Reason}";
}

public sealed class LotApprovedTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.LotApproved;

    public EmailPolicy EmailPolicy => EmailPolicy.IfOptedIn;

    public string BuildText(NotificationContext context) =>
        $"Посилання на збір схвалено, торги за «{context.LotTitle}» розпочалися";
}

public sealed class LotRejectedTemplate : INotificationTemplate
{
    public NotificationType Type => NotificationType.LotRejected;

    public EmailPolicy EmailPolicy => EmailPolicy.IfOptedIn;

    public string BuildText(NotificationContext context) =>
        $"Посилання на збір для «{context.LotTitle}» відхилено. Причина: {context.Reason}";
}
