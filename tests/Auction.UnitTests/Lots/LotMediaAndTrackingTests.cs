// Small lot-owned rows: photos, favorites and view keys.
using Auction.Core.Lots.Domain;

namespace Auction.UnitTests.Lots;

public class LotMediaAndTrackingTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Photo_at_position_zero_is_the_main_one()
    {
        var photo = LotPhoto.Add(1, 0, "lots/a-large.webp", "lots/a-thumb.webp", 1600, 1200, Now);

        Assert.True(photo.IsMain);
    }

    [Fact]
    public void Moving_a_photo_changes_its_position()
    {
        var photo = LotPhoto.Add(1, 0, "lots/a-large.webp", "lots/a-thumb.webp", 1600, 1200, Now);

        photo.MoveTo(2);

        Assert.Equal(2, photo.Position);
        Assert.False(photo.IsMain);
    }

    [Fact]
    public void Favorite_reminder_is_remembered()
    {
        var favorite = Favorite.Add(1, 2, Now);

        favorite.MarkReminderSent(Now.AddHours(5));

        Assert.Equal(Now.AddHours(5), favorite.ReminderSentAt);
    }

    [Fact]
    public void Visitor_keys_distinguish_users_from_anonymous_visitors()
    {
        Assert.Equal("u:42", LotView.UserKey(42));
        Assert.Equal("a:abc", LotView.AnonymousKey("abc"));
    }
}
