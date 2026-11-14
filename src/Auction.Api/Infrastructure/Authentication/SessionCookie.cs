// Writes and removes the HttpOnly, Secure, SameSite=Lax session cookie.
namespace Auction.Api.Infrastructure.Authentication;

internal static class SessionCookie
{
    public static void Write(HttpResponse response, SessionAuthenticationOptions options, string token) =>
        response.Cookies.Append(options.CookieName, token, Build(options.Lifetime));

    public static void Delete(HttpResponse response, SessionAuthenticationOptions options) =>
        response.Cookies.Delete(options.CookieName, Build(null));

    private static CookieOptions Build(TimeSpan? maxAge) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Path = "/",
        MaxAge = maxAge
    };
}
