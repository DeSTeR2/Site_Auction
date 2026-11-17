// Access checks every application service runs before an action (project.md §5, §9.11).
using Auction.Core.Common;

namespace Auction.Core.Platform.Contracts;

public interface IAccessGuard
{
    Result<Unit> EnsureAuthenticated();

    Result<Unit> EnsureEmailConfirmed();

    Result<Unit> EnsureAdmin();

    Result<Unit> EnsureOwner(long ownerId);
}
