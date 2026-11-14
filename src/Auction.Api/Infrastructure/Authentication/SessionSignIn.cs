// Starts a session after login or registration and ends it on logout (design/07-auth-security.md §1).
using Auction.Core.Common.Security;
using Auction.Core.Users.Contracts;
using Microsoft.Extensions.Options;

namespace Auction.Api.Infrastructure.Authentication;

public interface ISessionSignIn
{
    Task SignInAsync(HttpContext httpContext, long userId, CancellationToken ct);

    Task SignOutAsync(HttpContext httpContext, CancellationToken ct);
}

public sealed class SessionSignIn(
    ISessionStore sessions,
    TimeProvider clock,
    IOptionsMonitor<SessionAuthenticationOptions> optionsMonitor) : ISessionSignIn
{
    private SessionAuthenticationOptions Options => optionsMonitor.Get(SessionAuthenticationHandler.SchemeName);

    public async Task SignInAsync(HttpContext httpContext, long userId, CancellationToken ct)
    {
        var token = SecureToken.Generate();
        var now = clock.GetUtcNow();
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

        await sessions.CreateAsync(userId, SecureToken.Hash(token)!, now, now + Options.Lifetime, userAgent, ct);
        SessionCookie.Write(httpContext.Response, Options, token);
    }

    public async Task SignOutAsync(HttpContext httpContext, CancellationToken ct)
    {
        if (httpContext.Request.Cookies.TryGetValue(Options.CookieName, out var token)
            && SecureToken.Hash(token) is { } tokenHash)
        {
            await sessions.DeleteAsync(tokenHash, ct);
        }

        SessionCookie.Delete(httpContext.Response, Options);
    }
}
