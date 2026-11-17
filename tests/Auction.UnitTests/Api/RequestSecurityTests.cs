// CSRF origin check and the per-IP limit on /api/auth/* (design/07-auth-security.md §5).
using System.Net;

namespace Auction.UnitTests.Api;

public class RequestSecurityTests(AuctionApiFactory factory) : IClassFixture<AuctionApiFactory>
{
    [Fact]
    public async Task Post_from_foreign_origin_is_forbidden()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/test/public");
        request.Headers.Add("Origin", "https://evil.example");

        var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("access.forbidden", await response.ReadCodeAsync());
    }

    [Fact]
    public async Task Post_from_same_origin_or_without_origin_is_allowed()
    {
        var sameOrigin = new HttpRequestMessage(HttpMethod.Post, "/api/test/public");
        sameOrigin.Headers.Add("Origin", "http://localhost");

        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.NoContent, (await client.SendAsync(sameOrigin)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/test/public", null)).StatusCode);
    }

    [Fact]
    public async Task Get_from_foreign_origin_is_allowed()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/test/public");
        request.Headers.Add("Origin", "https://evil.example");

        var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class AuthRateLimitTests(StrictRateLimitApiFactory factory) : IClassFixture<StrictRateLimitApiFactory>
{
    [Fact]
    public async Task Auth_endpoints_are_limited_per_client()
    {
        var client = factory.CreateClient();

        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/auth/ping", null)).StatusCode);
        }

        var limited = await client.PostAsync("/api/auth/ping", null);

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Equal("auth.rate_limited", await limited.ReadCodeAsync());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/test/public")).StatusCode);
    }
}
