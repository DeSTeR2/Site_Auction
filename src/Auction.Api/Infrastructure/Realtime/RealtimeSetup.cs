// SignalR with a Redis backplane so every app instance behind Caddy reaches every lot watcher.
using Auction.Api.Hubs;
using StackExchange.Redis;

namespace Auction.Api.Infrastructure.Realtime;

public static class RealtimeSetup
{
    private const string BackplaneChannelPrefix = "auction";

    public static IServiceCollection AddRealtime(this IServiceCollection services, IConfiguration configuration)
    {
        var signalR = services.AddSignalR();

        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
        {
            signalR.AddStackExchangeRedis(redis, options =>
                options.Configuration.ChannelPrefix = RedisChannel.Literal(BackplaneChannelPrefix));
        }

        return services;
    }

    public static IEndpointRouteBuilder MapRealtime(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<AuctionHub>(AuctionHub.Path);
        return endpoints;
    }
}
