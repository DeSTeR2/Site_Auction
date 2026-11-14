// Default access checks; a foreign or missing resource is reported as "not found" (project.md §9.11).
using Auction.Core.Common;
using Auction.Core.Platform.Contracts;

namespace Auction.Core.Platform.Application;

public sealed class AccessGuard(ICurrentUser currentUser) : IAccessGuard
{
    public Result<Unit> EnsureAuthenticated() =>
        currentUser.IsAuthenticated ? Result.Success() : AppErrors.Auth.Required;

    public Result<Unit> EnsureEmailConfirmed()
    {
        var authenticated = EnsureAuthenticated();
        if (!authenticated.IsSuccess)
        {
            return authenticated;
        }

        return currentUser.IsEmailConfirmed ? Result.Success() : AppErrors.Access.EmailNotConfirmed;
    }

    public Result<Unit> EnsureAdmin()
    {
        var authenticated = EnsureAuthenticated();
        if (!authenticated.IsSuccess)
        {
            return authenticated;
        }

        return currentUser.IsAdmin ? Result.Success() : AppErrors.Access.Forbidden;
    }

    public Result<Unit> EnsureOwner(long ownerId)
    {
        var authenticated = EnsureAuthenticated();
        if (!authenticated.IsSuccess)
        {
            return authenticated;
        }

        return currentUser.UserId == ownerId ? Result.Success() : AppErrors.NotFound;
    }
}
