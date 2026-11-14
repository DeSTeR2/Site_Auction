// Moderation actions on lots used by the Users module when complaints are accepted.
using Auction.Core.Common;

namespace Auction.Core.Lots.Contracts;

public interface ILotModeration
{
    Task<Result<Unit>> HideAsync(long lotId, string reason, CancellationToken ct);

    Task<Result<Unit>> RestoreAsync(long lotId, CancellationToken ct);
}
