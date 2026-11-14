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
            || !request.Headers.TryGetValue(HeaderNames.Origin, out var o)
            || Same(o.ToString(), request.Host))
        {
            await next(context);
            return;
        }

        await context.WriteProblemAsync(AppErrors.Access.Forbidden);
    }

    private bool Same(string o, HostString h)
    {
        if (!Uri.TryCreate(o, UriKind.Absolute, out var u))
        {
            return false;
        }

        return string.Equals(u.Authority, h.Value, StringComparison.OrdinalIgnoreCase)
            || options.Value.AllowedOrigins.Contains(o.TrimEnd('/'), StringComparer.OrdinalIgnoreCase);
    }
}
