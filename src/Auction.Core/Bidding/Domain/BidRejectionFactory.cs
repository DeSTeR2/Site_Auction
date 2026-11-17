// Turns a bid rejection reason into an AppError with the code, status and text from project.md §8.8.
using Auction.Core.Bidding.Contracts;
using Auction.Core.Common;

namespace Auction.Core.Bidding.Domain;

public sealed record BidRejectionData(long MinimumBid, long CurrentPrice, long MaxBidAmount);

public static class BidRejectionFactory
{
    public static AppError Create(BidRejectionReason reason, BidRejectionData? data = null) => reason switch
    {
        BidRejectionReason.TooLow => new("bid.too_low", 422,
            $"Мінімальна ставка: {Money.Format(Require(data).MinimumBid)}",
            new { minimumBid = Require(data).MinimumBid }),
        BidRejectionReason.PriceChanged => new("bid.price_changed", 409,
            $"Поки ви ставили, ціна змінилася. Поточна: {Money.Format(Require(data).CurrentPrice)}, мінімальна: {Money.Format(Require(data).MinimumBid)}",
            new { currentPrice = Require(data).CurrentPrice, minimumBid = Require(data).MinimumBid }),
        BidRejectionReason.InvalidAmount => new("bid.invalid_amount", 400,
            $"Введіть цілу суму від {Require(data).MinimumBid} до {Money.Format(Require(data).MaxBidAmount)}"),
        BidRejectionReason.OwnLot => new("bid.own_lot", 403, "Не можна робити ставки на власний лот"),
        BidRejectionReason.AlreadyLeader => new("bid.already_leader", 409, "Ваша ставка вже найвища"),
        BidRejectionReason.AuctionEnded => new("bid.auction_ended", 409, "Торги завершено"),
        BidRejectionReason.LotUnavailable => new("bid.lot_unavailable", 409, "Лот недоступний"),
        BidRejectionReason.RateLimited => new("bid.rate_limited", 429, "Забагато ставок. Спробуйте за хвилину"),
        BidRejectionReason.EmailNotConfirmed => new("bid.email_not_confirmed", 403, "Підтвердіть email, щоб робити ставки"),
        BidRejectionReason.AuthenticationRequired => AppErrors.Auth.Required,
        BidRejectionReason.Forbidden => AppErrors.Access.Forbidden,
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unknown bid rejection reason.")
    };

    private static BidRejectionData Require(BidRejectionData? data) =>
        data ?? throw new ArgumentNullException(nameof(data), "This rejection reason needs bid amounts.");
}
