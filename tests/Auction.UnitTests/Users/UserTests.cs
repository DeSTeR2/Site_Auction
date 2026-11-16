// User aggregate: registration defaults, admin creation and blocking events.
using Auction.Core.Common.Enums;
using Auction.Core.Users.Contracts;
using Auction.Core.Users.Domain;

namespace Auction.UnitTests.Users;

public class UserTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_creates_active_unconfirmed_user_with_emails_enabled()
    {
        var user = User.Register("anna@example.com", "anna_k", "hash", Now);

        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.False(user.IsEmailConfirmed);
        Assert.True(user.EmailNotifications);
        Assert.Equal(Now, user.CreatedAt);
    }

    [Fact]
    public void CreateAdmin_has_admin_role_and_confirmed_email()
    {
        var admin = User.CreateAdmin("admin@example.com", "admin", "hash", Now);

        Assert.Equal(UserRole.Admin, admin.Role);
        Assert.True(admin.IsEmailConfirmed);
    }

    [Fact]
    public void ConfirmEmail_keeps_the_first_confirmation_time()
    {
        var user = User.Register("anna@example.com", "anna_k", "hash", Now);

        user.ConfirmEmail(Now);
        user.ConfirmEmail(Now.AddHours(1));

        Assert.Equal(Now, user.EmailConfirmedAt);
    }

    [Fact]
    public void Block_marks_user_blocked_and_raises_one_event()
    {
        var user = User.Register("anna@example.com", "anna_k", "hash", Now);

        user.Block(adminId: 7, "fake bids", Now);
        user.Block(adminId: 7, "fake bids", Now);

        Assert.True(user.IsBlocked);
        var blocked = Assert.IsType<UserBlocked>(Assert.Single(user.DomainEvents));
        Assert.Equal(7, blocked.AdminId);
        Assert.Equal("fake bids", blocked.Reason);
    }

    [Fact]
    public void Unblock_returns_user_to_active()
    {
        var user = User.Register("anna@example.com", "anna_k", "hash", Now);
        user.Block(adminId: 7, "fake bids", Now);

        user.Unblock();

        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Fact]
    public void UpdateProfile_replaces_profile_fields()
    {
        var user = User.Register("anna@example.com", "anna_k", "hash", Now);

        user.UpdateProfile("anna_new", "+380501234567", "Київ", emailNotifications: false);

        Assert.Equal("anna_new", user.Nickname);
        Assert.Equal("+380501234567", user.Phone);
        Assert.Equal("Київ", user.City);
        Assert.False(user.EmailNotifications);
    }
}
