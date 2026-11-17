// Session cookie authentication: valid, expired and blocked sessions, sliding renewal, sign-in and sign-out.
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Auction.Api.Infrastructure.Authentication;
using Auction.Core.Common.Enums;

namespace Auction.UnitTests.Api;

public class SessionAuthenticationTests(AuctionApiFactory factory) : IClassFixture<AuctionApiFactory>
{
    [Fact]
    public async Task Valid_session_identifies_the_user()
    {
        var token = factory.Sessions.Add(userId: 41, lastSeenAt: factory.Clock.GetUtcNow());

        var response = await factory.CreateClientWithSession(token).GetAsync("/api/test/authenticated");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(41, body.GetProperty("userId").GetInt64());
    }

    [Fact]
    public async Task Expired_session_is_treated_as_guest()
    {
        var now = factory.Clock.GetUtcNow();
        var token = factory.Sessions.Add(userId: 42, lastSeenAt: now.AddDays(-31), expiresAt: now.AddDays(-1));

        var response = await factory.CreateClientWithSession(token).GetAsync("/api/test/authenticated");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("auth.required", await response.ReadCodeAsync());
    }

    [Fact]
    public async Task Blocked_user_session_is_treated_as_guest()
    {
        var token = factory.Sessions.Add(userId: 43, lastSeenAt: factory.Clock.GetUtcNow(), status: UserStatus.Blocked);

        var response = await factory.CreateClientWithSession(token).GetAsync("/api/test/authenticated");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_token_is_treated_as_guest()
    {
        var response = await factory.CreateClientWithSession("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA").GetAsync("/api/test/public");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Null, body.GetProperty("userId").ValueKind);
    }

    [Fact]
    public async Task Long_user_agent_is_truncated_before_it_is_stored()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", new string('x', 4000));

        await client.PostAsync("/api/test/sign-in/48", null);

        Assert.Equal(SessionSignIn.MaxUserAgentLength, factory.Sessions.LastUserAgent!.Length);
    }

    [Fact]
    public async Task Session_seen_more_than_five_minutes_ago_is_renewed()
    {
        var token = factory.Sessions.Add(userId: 44, lastSeenAt: factory.Clock.GetUtcNow().AddMinutes(-6));

        var response = await factory.CreateClientWithSession(token).GetAsync("/api/test/authenticated");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), c => c.StartsWith(AuctionApiFactory.SessionCookie + "=" + token));
        Assert.NotEmpty(factory.Sessions.ExtendedSessionIds);
    }

    [Fact]
    public async Task Recent_session_is_not_written_back()
    {
        var token = factory.Sessions.Add(userId: 45, lastSeenAt: factory.Clock.GetUtcNow().AddMinutes(-1));

        var response = await factory.CreateClientWithSession(token).GetAsync("/api/test/authenticated");

        Assert.False(response.Headers.Contains("Set-Cookie") && response.Headers.GetValues("Set-Cookie").Any(c => c.StartsWith(AuctionApiFactory.SessionCookie)));
    }

    [Fact]
    public async Task Sign_in_sets_secure_http_only_cookie_and_sign_out_removes_session()
    {
        var client = factory.CreateClient();

        var signIn = await client.PostAsync("/api/test/sign-in/46", null);
        var cookie = Assert.Single(signIn.Headers.GetValues("Set-Cookie"), c => c.StartsWith(AuctionApiFactory.SessionCookie));
        var token = cookie.Split(';')[0].Split('=', 2)[1];

        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);

        var sessionClient = factory.CreateClientWithSession(token);
        Assert.Equal(HttpStatusCode.OK, (await sessionClient.GetAsync("/api/test/authenticated")).StatusCode);

        await sessionClient.PostAsync("/api/test/sign-out", null);
        Assert.Equal(HttpStatusCode.Unauthorized, (await sessionClient.GetAsync("/api/test/authenticated")).StatusCode);
    }
}
