// Donation link validation and normalization (design/14-charity.md §2).
using Auction.Core.Common;
using Auction.Core.Lots.Domain.Donations;

namespace Auction.UnitTests.Lots;

public class DonationMethodFactoryTests
{
    private readonly DonationMethodFactory factory = new();

    [Fact]
    public void Https_link_is_normalized_for_lookup()
    {
        var result = factory.Create(DonationMethodKind.Link, "  https://www.Example.com/Fund/?utm_source=ig&id=5&fbclid=abc#top ");

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/Fund?id=5", result.Value.NormalizedKey);
        Assert.Equal(DonationMethodKind.Link, result.Value.Kind);
    }

    [Theory]
    [InlineData("http://example.com/fund")]
    [InlineData("ftp://example.com/fund")]
    [InlineData("not a link")]
    [InlineData("")]
    public void Non_https_or_invalid_link_is_rejected(string raw)
    {
        var result = factory.Create(DonationMethodKind.Link, raw);

        Assert.Equal(AppErrors.Donation.LinkInvalid, result.Error);
    }

    [Fact]
    public void Same_fund_with_different_tracking_normalizes_to_same_key()
    {
        var first = factory.Create(DonationMethodKind.Link, "https://send.monobank.ua/jar/abc?utm_campaign=x").Value;
        var second = factory.Create(DonationMethodKind.Link, "https://send.monobank.ua/jar/abc/").Value;

        Assert.Equal(first.NormalizedKey, second.NormalizedKey);
    }
}
