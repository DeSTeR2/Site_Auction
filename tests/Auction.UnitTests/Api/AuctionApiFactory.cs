// In-memory host of the real API pipeline with a fake session store and a controllable clock.
using System.Net.Http.Json;
using System.Text.Json;
using Auction.Core.Users.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Auction.UnitTests.Api;

public class AuctionApiFactory : WebApplicationFactory<Program>
{
    public const string SessionCookie = "auction_session";

    public FakeSessionStore Sessions { get; } = new();

    public FakeTimeProvider Clock { get; } = new(TestLots.Start);

    protected virtual int AuthRequestsPerMinute => 20;

    public HttpClient CreateClientWithSession(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", $"{SessionCookie}={token}");
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Security:AuthRequestsPerMinute", AuthRequestsPerMinute.ToString());
        builder.UseSetting("Jobs:Storage", "InMemory");
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<ISessionStore>(Sessions);
            services.AddSingleton<TimeProvider>(Clock);
            services.AddSingleton<IValidator<EchoRequest>, EchoRequestValidator>();
            services.AddControllers().AddApplicationPart(typeof(TestEndpointsController).Assembly);
        });
    }
}

public sealed class StrictRateLimitApiFactory : AuctionApiFactory
{
    protected override int AuthRequestsPerMinute => 3;
}

internal static class ProblemResponse
{
    public static async Task<JsonElement> ReadProblemAsync(this HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    public static async Task<string?> ReadCodeAsync(this HttpResponseMessage response) =>
        (await response.ReadProblemAsync()).GetProperty("code").GetString();
}
