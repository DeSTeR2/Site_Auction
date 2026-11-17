// Who follows a lot; used by Bidding for "favorite ending soon" reminders.
namespace Auction.Core.Lots.Contracts;

public interface IFavoriteReader
{
    Task<IReadOnlyList<long>> GetFollowerIdsAsync(long lotId, CancellationToken ct);
}
