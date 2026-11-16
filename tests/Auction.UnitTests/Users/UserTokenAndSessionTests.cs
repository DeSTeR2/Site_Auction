// One-time tokens and sessions: expiry, single use and user agent truncation.
using Auction.Core.Common.Enums;
using Auction.Core.Users.Domain;

namespace Auction.UnitTests.Users;

public class UserTokenAndSessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Token_is_usable_until_it_expires()
    {
        var token = UserToken.Issue(1, TokenPurpose.EmailConfirm, [1, 2, 3], Now, TimeSpan.FromHours(24));

        Assert.True(token.IsUsable(Now.AddHours(23)));
        Assert.False(token.IsUsable(Now.AddHours(25)));
    }

    [Fact]
    public void Token_cannot_be_used_twice()
    {
        var token = UserToken.Issue(1, TokenPurpose.PasswordReset, [1, 2, 3], Now, TimeSpan.FromHours(1));

        token.MarkUsed(Now.AddMinutes(5));

        Assert.False(token.IsUsable(Now.AddMinutes(6)));
        Assert.Equal(Now.AddMinutes(5), token.UsedAt);
    }

    [Fact]
    public void Session_expires_at_its_expiry_time()
    {
        var session = UserSession.Start(1, [1, 2, 3], Now, Now.AddDays(30), "agent");

        Assert.False(session.IsExpired(Now.AddDays(29)));
        Assert.True(session.IsExpired(Now.AddDays(31)));
    }

    [Fact]
    public void Extend_moves_last_seen_and_expiry()
    {
        var session = UserSession.Start(1, [1, 2, 3], Now, Now.AddDays(30), "agent");

        session.Extend(Now.AddDays(1), Now.AddDays(31));

        Assert.Equal(Now.AddDays(1), session.LastSeenAt);
        Assert.Equal(Now.AddDays(31), session.ExpiresAt);
    }

    [Fact]
    public void Long_user_agent_is_truncated()
    {
        var session = UserSession.Start(1, [1, 2, 3], Now, Now.AddDays(30), new string('a', 2000));

        Assert.Equal(UserSession.MaxUserAgentLength, session.UserAgent!.Length);
    }
}
