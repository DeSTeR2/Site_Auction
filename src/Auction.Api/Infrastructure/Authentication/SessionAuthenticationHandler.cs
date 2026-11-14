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

        var tokenHash = SecureToken.Hash(token);
        if (tokenHash is null)
        {
            SessionCookie.Delete(Response, Options);
            return AuthenticateResult.NoResult();
        }

        var session = await sessions.FindByTokenHashAsync(tokenHash, Context.RequestAborted);
        var now = clock.GetUtcNow();
        if (session is null || session.ExpiresAt <= now || session.UserStatus != UserStatus.Active)
        {
            SessionCookie.Delete(Response, Options);
            return AuthenticateResult.NoResult();
        }

        if (now - session.LastSeenAt >= Options.RenewAfter)
        {
            try
            {
                await sessions.ExtendAsync(session.SessionId, now, now + Options.Lifetime, Context.RequestAborted);
                SessionCookie.Write(Response, Options, token);
            }
            catch (Exception)
            {
            }
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Role, session.Role == UserRole.Admin ? AuctionClaimTypes.AdminRole : AuctionClaimTypes.UserRole),
            new(AuctionClaimTypes.EmailConfirmed, session.IsEmailConfirmed ? bool.TrueString : bool.FalseString),
            new(AuctionClaimTypes.SessionId, session.SessionId.ToString(CultureInfo.InvariantCulture))
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
