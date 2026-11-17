// Authenticates requests by the session cookie: hash lookup, expiry and status check, sliding renewal.
using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Auction.Core.Common.Enums;
using Auction.Core.Common.Security;
using Auction.Core.Users.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Auction.Api.Infrastructure.Authentication;

public sealed class SessionAuthenticationHandler(
    IOptionsMonitor<SessionAuthenticationOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder,
    ISessionStore sessions,
    TimeProvider clock) : AuthenticationHandler<SessionAuthenticationOptions>(options, loggerFactory, encoder)
{
    public const string SchemeName = "Session";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(Options.CookieName, out var token))
        {
            return AuthenticateResult.NoResult();
        }

        var session = await FindActiveSessionAsync(token);
        if (session is null)
        {
            SessionCookie.Delete(Response, Options);
            return AuthenticateResult.NoResult();
        }

        await RenewIfStaleAsync(session, token);
        return AuthenticateResult.Success(CreateTicket(session));
    }

    private async Task<SessionRecord?> FindActiveSessionAsync(string token)
    {
        var tokenHash = SecureToken.Hash(token);
        if (tokenHash is null)
        {
            return null;
        }

        var session = await sessions.FindByTokenHashAsync(tokenHash, Context.RequestAborted);
        var isUsable = session is not null
            && session.ExpiresAt > clock.GetUtcNow()
            && session.UserStatus == UserStatus.Active;
        return isUsable ? session : null;
    }

    private async Task RenewIfStaleAsync(SessionRecord session, string token)
    {
        var now = clock.GetUtcNow();
        if (now - session.LastSeenAt < Options.RenewAfter)
        {
            return;
        }

        try
        {
            await sessions.ExtendAsync(session.SessionId, now, now + Options.Lifetime, Context.RequestAborted);
            SessionCookie.Write(Response, Options, token);
        }
        catch (OperationCanceledException) when (Context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Failed to renew session {SessionId}; the request continues with the current expiry", session.SessionId);
        }
    }

    private AuthenticationTicket CreateTicket(SessionRecord session)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Role, session.Role == UserRole.Admin ? AuctionClaimTypes.AdminRole : AuctionClaimTypes.UserRole),
            new(AuctionClaimTypes.EmailConfirmed, session.IsEmailConfirmed ? bool.TrueString : bool.FalseString),
            new(AuctionClaimTypes.SessionId, session.SessionId.ToString(CultureInfo.InvariantCulture))
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return new AuthenticationTicket(principal, Scheme.Name);
    }
}
