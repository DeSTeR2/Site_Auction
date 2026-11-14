// Access guard outcomes for guests, users, unconfirmed users and admins (project.md §9.11).
using Auction.Core.Common;
using Auction.Core.Common.Enums;
using Auction.Core.Platform.Application;
using Auction.Core.Platform.Contracts;

namespace Auction.UnitTests.Platform;

public class AccessGuardTests
{
    [Fact]
    public void Guest_is_asked_to_sign_in()
    {
        var guard = new AccessGuard(new FakeCurrentUser(null, UserRole.Guest, false));

        Assert.Equal(AppErrors.Auth.Required, guard.EnsureAuthenticated().Error);
        Assert.Equal(AppErrors.Auth.Required, guard.EnsureAdmin().Error);
        Assert.Equal(AppErrors.Auth.Required, guard.EnsureOwner(1).Error);
    }

    [Fact]
    public void User_without_confirmed_email_is_rejected()
    {
        var guard = new AccessGuard(new FakeCurrentUser(5, UserRole.User, false));

        Assert.Equal(AppErrors.Access.EmailNotConfirmed, guard.EnsureEmailConfirmed().Error);
    }

    [Fact]
    public void Foreign_resource_looks_like_not_found()
    {
        var guard = new AccessGuard(new FakeCurrentUser(5, UserRole.User, true));

        Assert.True(guard.EnsureOwner(5).IsSuccess);
        Assert.Equal(AppErrors.NotFound, guard.EnsureOwner(6).Error);
    }

    [Fact]
    public void Only_admin_passes_admin_check()
    {
        var user = new AccessGuard(new FakeCurrentUser(5, UserRole.User, true));
        var admin = new AccessGuard(new FakeCurrentUser(1, UserRole.Admin, true));

        Assert.Equal(AppErrors.Access.Forbidden, user.EnsureAdmin().Error);
        Assert.True(admin.EnsureAdmin().IsSuccess);
    }

    private sealed class FakeCurrentUser(long? userId, UserRole role, bool emailConfirmed) : ICurrentUser
    {
        public long? UserId => userId;

        public UserRole Role => role;

        public bool IsAuthenticated => userId is not null;

        public bool IsEmailConfirmed => emailConfirmed;

        public bool IsAdmin => role == UserRole.Admin;

        public string VisitorKey => userId is null ? "a:test" : $"u:{userId}";
    }
}
