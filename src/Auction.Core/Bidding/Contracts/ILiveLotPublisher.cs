// Pushes the current lot state to SignalR groups after a commit (design/05-realtime.md).
namespace Auction.Core.Bidding.Contracts;

public interface ILiveLotPublisher
{
    Task PublishStateAsync(long lotId, CancellationToken ct);
}
