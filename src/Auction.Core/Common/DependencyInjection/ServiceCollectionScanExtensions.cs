// Registers every concrete implementation of a contract found in an assembly (strategies, templates, rules).
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Auction.Core.Common.DependencyInjection;

public static class ServiceCollectionScanExtensions
{
    public static IServiceCollection AddImplementationsOf<TService>(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime)
    {
        var implementations = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(TService).IsAssignableFrom(t));

        foreach (var implementation in implementations)
        {
            services.Add(new ServiceDescriptor(typeof(TService), implementation, lifetime));
        }

        return services;
    }
}
