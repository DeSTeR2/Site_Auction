// Service registration of the Platform module, shared AuctionRules options and request validators.
using Auction.Core.Common.Options;
using Auction.Core.Platform.Application;
using Auction.Core.Platform.Contracts;
using FluentValidation;
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
        services.AddValidatorsFromAssembly(typeof(PlatformModule).Assembly, ServiceLifetime.Singleton);
        return services;
    }
}
