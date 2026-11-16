// EF Core context; entity configurations live per module under Persistence/<Module>/, conventions follow design/03-data-model.md §1.
using Auction.Core.Common.Events;
using Auction.Core.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Persistence;

public sealed class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options), IAuctionDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.UseIdentityAlwaysColumns();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuctionDbContext).Assembly);

        var entitiesWithEvents = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(IHasDomainEvents).IsAssignableFrom(t.ClrType))
            .Select(t => t.ClrType)
            .ToList();

        foreach (var clrType in entitiesWithEvents)
        {
            modelBuilder.Entity(clrType).Ignore(nameof(IHasDomainEvents.DomainEvents));
        }

        var enumProperties = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => (Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType).IsEnum)
            .ToList();

        foreach (var property in enumProperties)
        {
            property.SetValueConverter(SnakeCaseEnumConverter.For(Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType));
        }
    }
}
