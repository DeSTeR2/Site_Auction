// One-off commands run by deployment: "migrate" applies EF Core migrations, "seed" loads demo data outside production.
using Auction.Core.Common.Persistence;
using Auction.Core.Platform.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Auction.Api.Cli;

public static class CliCommands
{
    public const string Migrate = "migrate";
    public const string Seed = "seed";

    public static bool IsKnown(string command) => command is Migrate or Seed;

    public static async Task<int> RunAsync(WebApplication app, string command, CancellationToken ct = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(CliCommands));

        switch (command)
        {
            case Migrate:
                await services.GetRequiredService<IAuctionDbContext>().Database.MigrateAsync(ct);
                logger.LogInformation("Database migrations applied");
                return 0;

            case Seed when app.Environment.IsProduction():
                logger.LogError("The seed command is not allowed in production");
                return 1;

            case Seed:
                foreach (var seeder in services.GetServices<IDemoDataSeeder>().OrderBy(s => s.Order))
                {
                    await seeder.SeedAsync(ct);
                    logger.LogInformation("Demo data step {Seeder} finished", seeder.GetType().Name);
                }

                return 0;

            default:
                logger.LogError("Unknown command {Command}", command);
                return 2;
        }
    }
}
