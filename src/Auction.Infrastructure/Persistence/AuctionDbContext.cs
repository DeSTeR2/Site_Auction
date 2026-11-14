// EF Core context; entity configurations live per module under Persistence/<Module>/.
using Auction.Core.Common.Events;
using Auction.Core.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Persistence;

public sealed class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options), IAuctionDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuctionDbContext).Assembly);

        var entitiesWithEvents = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(IHasDomainEvents).IsAssignableFrom(t.ClrType))
            .Select(t => t.ClrType)
            .ToList();

        foreach (var clrType in entitiesWithEvents)
        {
            modelBuilder.Entity(clrType).Ignore(nameof(IHasDomainEvents.DomainEvents));
        }
    }
}
