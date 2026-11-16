// Service registration of the Bidding module (bids, rules, closing, notifications, suspicious activity).
using Auction.Core.Bidding.Application;
using Auction.Core.Bidding.Application.Notifications;
using Auction.Core.Bidding.Application.Rules;
using Auction.Core.Bidding.Contracts;
using Auction.Core.Common.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Auction.Core.Bidding;

public static class BiddingModule
{
    public static IServiceCollection AddBidding(this IServiceCollection services)
    {
        var assembly = typeof(BiddingModule).Assembly;

        services.AddOptions<RateLimitOptions>()
            .BindConfiguration(RateLimitOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddImplementationsOf<IBidRule>(assembly, ServiceLifetime.Singleton);
        services.AddSingleton<BidRuleChain>();

        services.AddImplementationsOf<INotificationTemplate>(assembly, ServiceLifetime.Singleton);
        services.AddSingleton<NotificationFactory>();

        services.AddScoped<ISuspiciousActivityLog, SuspiciousActivityLog>();
        return services;
    }
}
