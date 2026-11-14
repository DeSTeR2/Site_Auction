// ICurrentUser backed by the authenticated principal of the current HTTP request.
using System.Security.Claims;
using Auction.Core.Common.Enums;
using Auction.Core.Platform.Contracts;

namespace Auction.Api.Infrastructure;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public long? UserId =>
        long.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public bool IsAuthenticated => UserId is not null;

    public UserRole Role => !IsAuthenticated
        ? UserRole.Guest
        : Principal!.IsInRole(AuctionClaimTypes.AdminRole) ? UserRole.Admin : UserRole.User;

    public bool IsAdmin => Role == UserRole.Admin;

    public bool IsEmailConfirmed => Principal?.HasClaim(AuctionClaimTypes.EmailConfirmed, bool.TrueString) == true;

    public string VisitorKey => IsAuthenticated
        ? $"u:{UserId}"
        : $"a:{accessor.HttpContext?.Items[VisitorCookieMiddleware.ItemKey]}";
}
