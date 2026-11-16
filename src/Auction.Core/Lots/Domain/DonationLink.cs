// Global donation link an administrator approves, rejects or revokes (design/14-charity.md §3).
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.Core.Common.Events;

namespace Auction.Core.Lots.Domain;

public sealed class DonationLink : Entity
{
    private DonationLink()
    {
        NormalizedUrl = string.Empty;
        OriginalUrl = string.Empty;
    }

    public string NormalizedUrl { get; private set; }

    public string OriginalUrl { get; private set; }

    public DonationLinkStatus Status { get; private set; }

    public long SubmittedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public long? ReviewedBy { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public string? ReviewReason { get; private set; }

    public static DonationLink Submit(string normalizedUrl, string originalUrl, long submittedBy, DateTimeOffset now) => new()
    {
        NormalizedUrl = normalizedUrl,
        OriginalUrl = originalUrl,
        Status = DonationLinkStatus.Pending,
        SubmittedBy = submittedBy,
        CreatedAt = now
    };

    public Result<Unit> Approve(long adminId, DateTimeOffset now)
    {
        if (Status != DonationLinkStatus.Pending)
        {
            return AppErrors.Lot.InvalidTransition;
        }

        Status = DonationLinkStatus.Approved;
        ReviewedBy = adminId;
        ReviewedAt = now;
        return Result.Success();
    }

    public Result<Unit> Reject(long adminId, string reason, DateTimeOffset now)
    {
        if (Status != DonationLinkStatus.Pending)
        {
            return AppErrors.Lot.InvalidTransition;
        }

        Status = DonationLinkStatus.Rejected;
        ReviewedBy = adminId;
        ReviewedAt = now;
        ReviewReason = reason;
        return Result.Success();
    }

    public Result<Unit> Revoke(long adminId, string reason, DateTimeOffset now)
    {
        if (Status != DonationLinkStatus.Approved)
        {
            return AppErrors.Lot.InvalidTransition;
        }

        Status = DonationLinkStatus.Revoked;
        ReviewedBy = adminId;
        ReviewedAt = now;
        ReviewReason = reason;
        return Result.Success();
    }
}
