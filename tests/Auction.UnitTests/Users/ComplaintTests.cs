// Complaint rules: comment required for "other", review allowed once.
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.Core.Users.Domain;

namespace Auction.UnitTests.Users;

public class ComplaintTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Reason_other_without_comment_is_rejected()
    {
        var result = Complaint.Submit(1, 10, ComplaintReason.Other, "  ", Now);

        Assert.Equal(AppErrors.ValidationFailed, result.Error);
    }

    [Fact]
    public void Known_reason_does_not_need_a_comment()
    {
        var result = Complaint.Submit(1, 10, ComplaintReason.FakeBids, null, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ComplaintStatus.New, result.Value.Status);
    }

    [Fact]
    public void Accept_records_reviewer_and_outcome()
    {
        var complaint = Complaint.Submit(1, 10, ComplaintReason.FakeBids, null, Now).Value;

        var result = complaint.Accept(adminId: 7, "confirmed", Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(ComplaintStatus.Accepted, complaint.Status);
        Assert.Equal(7, complaint.ReviewedBy);
        Assert.Equal("confirmed", complaint.ReviewComment);
    }

    [Fact]
    public void Reviewed_complaint_cannot_be_reviewed_again()
    {
        var complaint = Complaint.Submit(1, 10, ComplaintReason.FakeBids, null, Now).Value;
        complaint.Reject(adminId: 7, null, Now.AddHours(1));

        var result = complaint.Accept(adminId: 8, null, Now.AddHours(2));

        Assert.False(result.IsSuccess);
        Assert.Equal(ComplaintStatus.Rejected, complaint.Status);
    }
}
