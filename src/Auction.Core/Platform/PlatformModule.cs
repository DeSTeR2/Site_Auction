// Service registration of the Platform module and the shared AuctionRules options.
using Auction.Core.Common.Options;
using Auction.Core.Platform.Application;
using Auction.Core.Platform.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Auction.Core.Platform;

public static class PlatformModule
{
    public static IServiceCollection AddPlatform(this IServiceCollection services)
    {
        services.AddOptions<AuctionRules>()
            .BindConfiguration(AuctionRules.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AuctionRules>>().Value);

        services.AddScoped<IAccessGuard, AccessGuard>();
        return services;
    }
}
