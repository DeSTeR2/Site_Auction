// Creates lots as a draft, as a relist copy or as demo data with the same invariants (design/11-patterns.md §1.3).
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.Core.Common.Options;

namespace Auction.Core.Lots.Domain;

public sealed record LotDraftInput(
    short CategoryId,
    string Title,
    string Description,
    LotCondition Condition,
    long StartPrice,
    long BidStep,
    int DurationMinutes,
    string DonationUrl);

public sealed record DemoLotSpec(LotDraftInput Draft, LotStatus Status, DateTimeOffset? StartedAt);

public sealed class LotFactory(TimeProvider clock, AuctionRules rules)
{
    private const int MinutesPerDay = 24 * 60;

    public Result<Lot> CreateDraft(long sellerId, LotDraftInput input)
    {
        if (!IsAllowedDuration(input.DurationMinutes))
        {
            return AppErrors.ValidationFailed;
        }

        return new Lot(sellerId, input, clock.GetUtcNow());
    }

    public Result<Lot> CreateRelistCopy(Lot source)
    {
        if (source.Status is not (LotStatus.Unsold or LotStatus.Withdrawn))
        {
            return AppErrors.Lot.InvalidTransition;
        }

        var input = new LotDraftInput(
            source.CategoryId,
            source.Title,
            source.Description,
            source.Condition,
            source.StartPrice,
            source.BidStep,
            source.DurationMinutes,
            source.DonationUrl);

        return new Lot(source.SellerId, input, clock.GetUtcNow(), source.Id);
    }

    public Lot CreateDemo(long sellerId, DemoLotSpec spec)
    {
        var lot = new Lot(sellerId, spec.Draft, spec.StartedAt ?? clock.GetUtcNow());
        lot.ForceDemoState(spec.Status, spec.StartedAt);
        return lot;
    }

    private bool IsAllowedDuration(int durationMinutes)
    {
        if (rules.AllowMinuteDurations)
        {
            return durationMinutes > 0;
        }

        return durationMinutes % MinutesPerDay == 0
            && rules.AllowedDurationsDays.Contains(durationMinutes / MinutesPerDay);
    }
}
