// Takes the row lock on a lot inside the caller's transaction (design/04-bidding-concurrency.md §1).
namespace Auction.Core.Lots.Contracts;

public interface ILotLocker
{
    Task<ILockedLot?> LockForUpdateAsync(long lotId, CancellationToken ct);
}
