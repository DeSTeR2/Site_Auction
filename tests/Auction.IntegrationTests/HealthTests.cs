// Health endpoints used by Caddy and the deploy script, checked against a real PostgreSQL.
using System.Net;
using Auction.IntegrationTests.Infrastructure;

namespace Auction.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class HealthTests(PostgresApiFixture fixture)
{
    [Fact]
    public async Task Readiness_is_healthy_when_database_is_reachable()
    {
        var response = await fixture.Factory.CreateClient().GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Liveness_does_not_depend_on_the_database()
    {
        var response = await fixture.Factory.CreateClient().GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
