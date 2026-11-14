// Catalogue of stable error codes from design/08-api.md §2; the frontend decides behaviour by code.
namespace Auction.Core.Common;

public static class AppErrors
{
    public static readonly AppError NotFound = new("not_found", 404, "Не знайдено");
    public static readonly AppError ValidationFailed = new("validation.failed", 400, "Перевірте поля форми");
    public static readonly AppError ServerError = new("server.error", 500, "Щось пішло не так");

    public static class Auth
    {
        public static readonly AppError Required = new("auth.required", 401, "Увійдіть, щоб продовжити");
        public static readonly AppError InvalidCredentials = new("auth.invalid_credentials", 401, "Невірний email або пароль");
        public static readonly AppError AccountBlocked = new("auth.account_blocked", 403, "Обліковий запис заблоковано");
        public static readonly AppError EmailTaken = new("auth.email_taken", 409, "Цей email уже зареєстровано");
        public static readonly AppError NicknameTaken = new("auth.nickname_taken", 409, "Цей нікнейм уже зайнято");
        public static readonly AppError EmailAlreadyConfirmed = new("auth.email_already_confirmed", 409, "Email уже підтверджено");
        public static readonly AppError TokenInvalid = new("auth.token_invalid", 400, "Посилання недійсне або застаріло");
        public static readonly AppError ResendTooSoon = new("auth.resend_too_soon", 429, "Лист можна надіслати повторно через хвилину");
        public static readonly AppError RateLimited = new("auth.rate_limited", 429, "Забагато спроб. Спробуйте за хвилину");
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
        public static readonly AppError MissingFields = new("lot.missing_fields", 400, "Заповніть обов'язкові поля");
        public static readonly AppError LastPhoto = new("lot.last_photo", 409, "Не можна видалити останнє фото");

        public static AppError ActiveLimit(int max) => new("lot.active_limit", 409, $"Можна мати не більше {max} активних лотів");

        public static AppError DraftLimit(int max) => new("lot.draft_limit", 409, $"Можна мати не більше {max} чернеток");
    }

    public static class Photo
    {
        public static readonly AppError Invalid = new("photo.invalid", 400, "Фото: JPEG, PNG або WebP, до 5 МБ, від 400×400");
    }

    public static class Donation
    {
        public static readonly AppError LinkInvalid = new("donation.link_invalid", 400, "Вкажіть посилання на збір, що починається з https://");
        public static readonly AppError LinkRejected = new("donation.link_rejected", 409, "Це посилання на збір не схвалено");
    }

    public static class Bid
    {
        public static readonly AppError Busy = new("bid.busy", 503, "Спробуйте ще раз");
    }

    public static class Favorite
    {
        public static AppError Limit(int max) => new("favorite.limit", 409, $"Можна мати не більше {max} обраних");
    }

    public static class Complaint
    {
        public static readonly AppError Duplicate = new("complaint.duplicate", 409, "Ви вже поскаржилися на цей лот");
    }
}
