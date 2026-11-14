// Shared error codes from design/08-api.md that are not owned by a single module.
namespace Auction.Core.Common;

public static class AppErrors
{
    public static readonly AppError NotFound = new("not_found", 404, "Не знайдено");
    public static readonly AppError ValidationFailed = new("validation.failed", 400, "Перевірте поля форми");
    public static readonly AppError ServerError = new("server.error", 500, "Щось пішло не так");

    public static class Auth
    {
        public static readonly AppError Required = new("auth.required", 401, "Увійдіть, щоб продовжити");
        public static readonly AppError AccountBlocked = new("auth.account_blocked", 403, "Обліковий запис заблоковано");
    }

    public static class Access
    {
        public static readonly AppError Forbidden = new("access.forbidden", 403, "Немає доступу");
        public static readonly AppError EmailNotConfirmed = new("access.email_not_confirmed", 403, "Підтвердіть email, щоб продовжити");
    }

    public static class Lot
    {
        public static readonly AppError InvalidTransition = new("lot.invalid_transition", 409, "Цю дію зараз виконати неможливо");
        public static readonly AppError HasBids = new("lot.has_bids", 409, "На лот уже зроблено ставку, редагування неможливе");
    }

    public static class Donation
    {
        public static readonly AppError LinkInvalid = new("donation.link_invalid", 400, "Вкажіть посилання на збір, що починається з https://");
    }
}
