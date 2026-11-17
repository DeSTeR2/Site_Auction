// LotFactory: durations from AuctionRules, relisting and demo lots.
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.Core.Common.Options;
using Auction.Core.Lots.Domain;

namespace Auction.UnitTests.Lots;

public class LotFactoryTests
{
    [Fact]
    public void Draft_with_allowed_duration_starts_as_draft_at_start_price()
    {
        var result = TestLots.Factory().CreateDraft(5, TestLots.Draft(durationMinutes: 7 * TestLots.OneDayMinutes));

        Assert.True(result.IsSuccess);
        Assert.Equal(LotStatus.Draft, result.Value.Status);
        Assert.Equal(100, result.Value.CurrentPrice);
        Assert.Equal(TestLots.Start, result.Value.CreatedAt);
    }

    [Fact]
    public void Draft_with_minute_duration_is_rejected_unless_allowed()
    {
        var strict = TestLots.Factory().CreateDraft(5, TestLots.Draft(durationMinutes: 15));
        var relaxed = TestLots.Factory(rules: new AuctionRules { AllowMinuteDurations = true })
            .CreateDraft(5, TestLots.Draft(durationMinutes: 15));

        Assert.Equal(AppErrors.ValidationFailed, strict.Error);
        Assert.True(relaxed.IsSuccess);
    }

    [Fact]
    public void Relist_copy_of_unsold_lot_is_a_new_draft_linked_to_source()
    {
        var source = TestLots.Active();
        source.Close(source.EndsAt!.Value);

        var copy = TestLots.Factory().CreateRelistCopy(source);

        Assert.True(copy.IsSuccess);
        Assert.Equal(LotStatus.Draft, copy.Value.Status);
        Assert.Equal(source.Title, copy.Value.Title);
        Assert.Equal(source.Id, copy.Value.SourceLotId);
    }

    [Fact]
    public void Relist_of_active_lot_is_rejected()
    {
        var copy = TestLots.Factory().CreateRelistCopy(TestLots.Active());

        Assert.Equal(AppErrors.Lot.InvalidTransition, copy.Error);
    }

    [Fact]
    public void Demo_lot_can_be_created_in_any_status()
    {
        var lot = TestLots.Factory().CreateDemo(3, new DemoLotSpec(TestLots.Draft(), LotStatus.Active, TestLots.Start));

        Assert.Equal(LotStatus.Active, lot.Status);
        Assert.Equal(TestLots.Start.AddDays(1), lot.EndsAt);
    }
}
