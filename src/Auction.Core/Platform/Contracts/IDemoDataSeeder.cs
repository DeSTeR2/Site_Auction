// Demo data step of a module (project.md §3.4); the seed command runs all steps in Order, never in production.
namespace Auction.Core.Platform.Contracts;

public interface IDemoDataSeeder
{
    int Order { get; }

    Task SeedAsync(CancellationToken ct);
}
