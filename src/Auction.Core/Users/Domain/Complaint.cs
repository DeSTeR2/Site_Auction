// Complaint about a lot from a user; an administrator moves it from new to accepted or rejected (project.md §6.8).
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.Core.Common.Events;

namespace Auction.Core.Users.Domain;

public sealed class Complaint : Entity
{
    private Complaint()
    {
    }

    public long AuthorId { get; private set; }

    public long LotId { get; private set; }

    public ComplaintReason Reason { get; private set; }

    public string? Comment { get; private set; }

    public ComplaintStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public long? ReviewedBy { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public string? ReviewComment { get; private set; }

    public static Result<Complaint> Submit(long authorId, long lotId, ComplaintReason reason, string? comment, DateTimeOffset now)
    {
        if (reason == ComplaintReason.Other && string.IsNullOrWhiteSpace(comment))
        {
            return AppErrors.ValidationFailed;
        }

        return new Complaint
        {
            AuthorId = authorId,
            LotId = lotId,
            Reason = reason,
            Comment = comment,
            Status = ComplaintStatus.New,
            CreatedAt = now
        };
    }

    public Result<Unit> Accept(long adminId, string? comment, DateTimeOffset now) => Review(ComplaintStatus.Accepted, adminId, comment, now);

    public Result<Unit> Reject(long adminId, string? comment, DateTimeOffset now) => Review(ComplaintStatus.Rejected, adminId, comment, now);

    private Result<Unit> Review(ComplaintStatus outcome, long adminId, string? comment, DateTimeOffset now)
    {
        if (Status != ComplaintStatus.New)
        {
            return AppErrors.Lot.InvalidTransition;
        }

        Status = outcome;
        ReviewedBy = adminId;
        ReviewedAt = now;
        ReviewComment = comment;
        return Result.Success();
    }
}
