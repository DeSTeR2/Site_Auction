// Notification factory: a template for every type and the email policy of §9.7.
using Auction.Core.Bidding;
using Auction.Core.Bidding.Application.Notifications;
using Auction.Core.Bidding.Contracts;
using Auction.Core.Common.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Auction.UnitTests.Bidding;

public class NotificationFactoryTests
{
    private static readonly NotificationContext Context = new(42, "Вінтажний фотоапарат", Amount: 1500, DonationUrl: "https://example.com/fund");

    private readonly NotificationFactory factory = BuildFactory();

    [Fact]
    public void Every_notification_type_has_a_template()
    {
        foreach (var type in Enum.GetValues<NotificationType>())
        {
            var notification = factory.Create(type, 7, Context, recipientOptedIn: true);

            Assert.Equal(type, notification.Type);
            Assert.False(string.IsNullOrWhiteSpace(notification.Text));
            Assert.Equal("/lots/42", notification.Link);
        }
    }

    [Theory]
    [InlineData(NotificationType.Outbid, true, EmailStatus.Pending)]
    [InlineData(NotificationType.Outbid, false, EmailStatus.None)]
    [InlineData(NotificationType.YouWon, false, EmailStatus.Pending)]
    [InlineData(NotificationType.LotSold, false, EmailStatus.Pending)]
    [InlineData(NotificationType.AccountBlocked, false, EmailStatus.Pending)]
    [InlineData(NotificationType.YouLost, true, EmailStatus.None)]
    [InlineData(NotificationType.NewBidOnYourLot, true, EmailStatus.None)]
    public void Email_policy_respects_opt_out_and_mandatory_types(NotificationType type, bool optedIn, EmailStatus expected)
    {
        var notification = factory.Create(type, 7, Context, optedIn);

        Assert.Equal(expected, notification.EmailStatus);
    }

    [Fact]
    public void You_won_text_contains_donation_link()
    {
        var notification = factory.Create(NotificationType.YouWon, 7, Context, recipientOptedIn: true);

        Assert.Contains("https://example.com/fund", notification.Text);
        Assert.Contains("1 500 грн", notification.Text);
    }

    private static NotificationFactory BuildFactory()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton<TimeProvider>(new FakeTimeProvider(TestLots.Start));
        services.AddBidding();
        return services.BuildServiceProvider().GetRequiredService<NotificationFactory>();
    }
}
