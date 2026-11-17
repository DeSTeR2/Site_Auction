// Registers the session authentication scheme, sign-in service and access policies.
using Auction.Api.Infrastructure.Authorization;
using Auction.Core.Users.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Auction.Api.Infrastructure.Authentication;

public static class AuthenticationSetup
{
    public static IServiceCollection AddSessionAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(SessionAuthenticationHandler.SchemeName)
            .AddScheme<SessionAuthenticationOptions, SessionAuthenticationHandler>(
                SessionAuthenticationHandler.SchemeName,
                options => configuration.GetSection(SessionAuthenticationOptions.SectionName).Bind(options));

        services.TryAddScoped<ISessionStore, NoSessionStore>();
        services.AddScoped<ISessionSignIn, SessionSignIn>();

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthorizationResultHandler>();
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.Authenticated, policy => policy.RequireAuthenticatedUser())
            .AddPolicy(AuthPolicies.EmailConfirmed, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(AuctionClaimTypes.EmailConfirmed, bool.TrueString))
            .AddPolicy(AuthPolicies.UserOnly, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(AuctionClaimTypes.UserRole))
            .AddPolicy(AuthPolicies.Admin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(AuctionClaimTypes.AdminRole));

        return services;
    }
}
