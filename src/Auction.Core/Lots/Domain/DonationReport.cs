// The winner's report that the final price was donated, with an optional public receipt (design/14-charity.md §4).
namespace Auction.Core.Lots.Domain;

public sealed class DonationReport
{
    private DonationReport()
    {
    }

    public long LotId { get; private set; }

    public long WinnerId { get; private set; }

    public long Amount { get; private set; }

    public string? ReceiptKey { get; private set; }

    public DateTimeOffset ReportedAt { get; private set; }

    public static DonationReport Create(long lotId, long winnerId, long amount, string? receiptKey, DateTimeOffset now) => new()
    {
        LotId = lotId,
        WinnerId = winnerId,
        Amount = amount,
        ReceiptKey = receiptKey,
        ReportedAt = now
    };
}
