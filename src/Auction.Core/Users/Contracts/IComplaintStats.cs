// Complaint counters for the admin metrics dashboard.
namespace Auction.Core.Users.Contracts;

public sealed record ComplaintCounts(int New, int Reviewed);

public interface IComplaintStats
{
    Task<ComplaintCounts> CountAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
}
