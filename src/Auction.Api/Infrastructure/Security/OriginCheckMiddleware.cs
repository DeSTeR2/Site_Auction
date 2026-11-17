// CSRF protection on top of SameSite=Lax: state-changing requests from a foreign Origin are rejected.
using Auction.Core.Common;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Auction.Api.Infrastructure.Security;

public sealed class OriginCheckMiddleware(RequestDelegate next, IOptions<SecurityOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        if (HttpMethods.IsGet(request.Method)
            || HttpMethods.IsHead(request.Method)
            || HttpMethods.IsOptions(request.Method)
            || !request.Headers.TryGetValue(HeaderNames.Origin, out var originHeader)
            || IsTrustedOrigin(originHeader.ToString(), request.Host))
        {
            await next(context);
            return;
        }

        await context.WriteProblemAsync(AppErrors.Access.Forbidden);
    }

    private bool IsTrustedOrigin(string origin, HostString host)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
        {
            return false;
        }

        return string.Equals(originUri.Authority, host.Value, StringComparison.OrdinalIgnoreCase)
            || options.Value.AllowedOrigins.Contains(origin.TrimEnd('/'), StringComparer.OrdinalIgnoreCase);
    }
}
