// Session cookie settings from design/07-auth-security.md §1, bound to "Security:Session".
using Microsoft.AspNetCore.Authentication;

namespace Auction.Api.Infrastructure.Authentication;

public sealed class SessionAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SectionName = "Security:Session";

    public string CookieName { get; set; } = "auction_session";

    public TimeSpan Lifetime { get; set; } = TimeSpan.FromDays(30);

    public TimeSpan RenewAfter { get; set; } = TimeSpan.FromMinutes(5);
}
