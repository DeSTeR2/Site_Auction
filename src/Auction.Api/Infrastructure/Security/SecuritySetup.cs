// Registers security options and the per-IP limiter for /api/auth/* (20 requests a minute by default).
using System.Threading.RateLimiting;
using Auction.Core.Common;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Auction.Api.Infrastructure.Security;

public static class SecuritySetup
{
    private const string AuthPathPrefix = "/api/auth";

    public static IServiceCollection AddRequestSecurity(this IServiceCollection services)
    {
        services.AddOptions<SecurityOptions>()
            .BindConfiguration(SecurityOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRateLimiter(_ => { });
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<SecurityOptions>>((limiter, security) =>
            {
                limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    context.Request.Path.StartsWithSegments(AuthPathPrefix)
                        ? RateLimitPartition.GetFixedWindowLimiter(
                            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = security.Value.AuthRequestsPerMinute,
                                Window = TimeSpan.FromMinutes(1),
                                QueueLimit = 0
                            })
                        : RateLimitPartition.GetNoLimiter(string.Empty));

                limiter.OnRejected = (rejected, _) =>
                    new ValueTask(rejected.HttpContext.WriteProblemAsync(AppErrors.Auth.RateLimited));
            });

        return services;
    }
}
