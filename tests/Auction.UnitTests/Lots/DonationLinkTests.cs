// Donation link review: pending links are approved or rejected, approved links can be revoked.
using Auction.Core.Common.Enums;
using Auction.Core.Lots.Domain;

namespace Auction.UnitTests.Lots;

public class DonationLinkTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);

    private static DonationLink NewLink() =>
        DonationLink.Submit("https://send.monobank.ua/abc", "https://www.send.monobank.ua/abc/?utm_source=x", submittedBy: 1, Now);

    [Fact]
    public void Submitted_link_waits_for_review()
    {
        var link = NewLink();

        Assert.Equal(DonationLinkStatus.Pending, link.Status);
        Assert.Null(link.ReviewedBy);
    }

    [Fact]
    public void Approve_moves_pending_link_to_approved()
    {
        var link = NewLink();

        var result = link.Approve(adminId: 7, Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(DonationLinkStatus.Approved, link.Status);
        Assert.Equal(7, link.ReviewedBy);
    }

    [Fact]
    public void Reject_keeps_the_reason()
    {
        var link = NewLink();

        link.Reject(adminId: 7, "not a charity", Now.AddHours(1));

        Assert.Equal(DonationLinkStatus.Rejected, link.Status);
        Assert.Equal("not a charity", link.ReviewReason);
    }

    [Fact]
    public void Revoke_moves_approved_link_to_revoked()
    {
        var link = NewLink();
        link.Approve(adminId: 7, Now.AddHours(1));

        var result = link.Revoke(adminId: 8, "fraud reports", Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(DonationLinkStatus.Revoked, link.Status);
        Assert.Equal(8, link.ReviewedBy);
    }

    [Fact]
    public void Approved_link_cannot_be_approved_again()
    {
        var link = NewLink();
        link.Approve(adminId: 7, Now.AddHours(1));

        var result = link.Approve(adminId: 8, Now.AddHours(2));

        Assert.False(result.IsSuccess);
        Assert.Equal(7, link.ReviewedBy);
    }
}
