// Service registration of the Users module (accounts, statistics, moderation).
using Microsoft.Extensions.DependencyInjection;

namespace Auction.Core.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsers(this IServiceCollection services) => services;
}
