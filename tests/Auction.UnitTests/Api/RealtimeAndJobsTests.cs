// The auction hub accepts guests and the background job dashboard opens only for administrators.
using System.Net;
using Auction.Api.Hubs;
using Auction.Api.Infrastructure.Jobs;
using Auction.Core.Common.Enums;

namespace Auction.UnitTests.Api;

public class RealtimeAndJobsTests(AuctionApiFactory factory) : IClassFixture<AuctionApiFactory>
{
    [Fact]
    public async Task Guest_can_negotiate_a_hub_connection()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync($"{AuctionHub.Path}/negotiate?negotiateVersion=1", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("connectionToken", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Guest_gets_auth_required_on_the_job_dashboard()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(JobsSetup.DashboardPath);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("auth.required", await response.ReadCodeAsync());
    }

    [Fact]
    public async Task Admin_opens_the_job_dashboard()
    {
        var client = factory.CreateClientWithSession(factory.Sessions.Add(1, factory.Clock.GetUtcNow(), UserRole.Admin));

        var response = await client.GetAsync(JobsSetup.DashboardPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }
}
