// Service registration of the Lots module (lots, catalog, search, favorites, charity).
using Auction.Core.Common.DependencyInjection;
using Auction.Core.Lots.Application.Catalog;
using Auction.Core.Lots.Domain;
using Auction.Core.Lots.Domain.Donations;
using Microsoft.Extensions.DependencyInjection;

namespace Auction.Core.Lots;

public static class LotsModule
{
    public static IServiceCollection AddLots(this IServiceCollection services)
    {
        var assembly = typeof(LotsModule).Assembly;

        services.AddSingleton<LotFactory>();
        services.AddSingleton<IDonationMethodFactory, DonationMethodFactory>();
        services.AddImplementationsOf<ICatalogSortStrategy>(assembly, ServiceLifetime.Singleton);
        services.AddImplementationsOf<IEndsWithinFilter>(assembly, ServiceLifetime.Singleton);
        services.AddSingleton<CatalogStrategyResolver>();
        return services;
    }
}
