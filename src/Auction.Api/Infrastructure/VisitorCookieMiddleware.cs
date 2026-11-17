// Gives every anonymous browser a stable visitor id cookie used for unique lot views.
namespace Auction.Api.Infrastructure;

public sealed class VisitorCookieMiddleware(RequestDelegate next)
{
    public const string CookieName = "auction_visitor";
    public const string ItemKey = "VisitorId";

    public Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue(CookieName, out var visitorId) || string.IsNullOrWhiteSpace(visitorId))
        {
            visitorId = Guid.NewGuid().ToString("N");
            context.Response.Cookies.Append(CookieName, visitorId, new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromDays(365)
            });
        }

        context.Items[ItemKey] = visitorId;
        return next(context);
    }
}
