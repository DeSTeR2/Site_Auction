// SignalR hub at /hubs/auction through which lot pages receive live bidding state (design/05-realtime.md).
using Microsoft.AspNetCore.SignalR;

namespace Auction.Api.Hubs;

public sealed class AuctionHub : Hub
{
    public const string Path = "/hubs/auction";
}
