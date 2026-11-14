// Deployment commands: seed runs demo steps in order and is refused in production.
using Auction.Api.Cli;
using Auction.Core.Platform.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Auction.UnitTests.Api;

public class CliCommandsTests
{
    [Fact]
    public async Task Seed_is_refused_in_production()
    {
        var calls = new List<string>();
        var app = BuildApp(Environments.Production, new RecordingSeeder("users", 1, calls));

        var exitCode = await CliCommands.RunAsync(app, CliCommands.Seed);

        Assert.Equal(1, exitCode);
        Assert.Empty(calls);
    }

    [Fact]
    public async Task Seed_runs_steps_in_order_outside_production()
    {
        var calls = new List<string>();
        var app = BuildApp(
            Environments.Development,
            new RecordingSeeder("lots", 2, calls),
            new RecordingSeeder("users", 1, calls));

        var exitCode = await CliCommands.RunAsync(app, CliCommands.Seed);

        Assert.Equal(0, exitCode);
        Assert.Equal(["users", "lots"], calls);
    }

    [Theory]
    [InlineData("migrate", true)]
    [InlineData("seed", true)]
    [InlineData("--urls=http://localhost", false)]
    [InlineData("deploy", false)]
    public void Only_known_commands_are_recognized(string argument, bool expected)
    {
        Assert.Equal(expected, CliCommands.IsKnown(argument));
    }

    private static WebApplication BuildApp(string environment, params IDemoDataSeeder[] seeders)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        foreach (var seeder in seeders)
        {
            builder.Services.AddSingleton(seeder);
        }

        return builder.Build();
    }

    private sealed class RecordingSeeder(string name, int order, List<string> calls) : IDemoDataSeeder
    {
        public int Order => order;

        public Task SeedAsync(CancellationToken ct)
        {
            calls.Add(name);
            return Task.CompletedTask;
        }
    }
}
