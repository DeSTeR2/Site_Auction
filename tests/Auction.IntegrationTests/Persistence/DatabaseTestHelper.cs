// Shared helpers for schema tests: migrating, saving entities, raw SQL and expected PostgreSQL error codes.
using Auction.Core.Common.Enums;
using Auction.Core.Common.Events;
using Auction.Core.Common.Options;
using Auction.Core.Lots.Domain;
using Auction.Core.Users.Domain;
using Auction.Infrastructure.Persistence;
using Auction.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Auction.IntegrationTests.Persistence;

public sealed class DatabaseTestHelper(PostgresApiFixture fixture)
{
    public static readonly DateTimeOffset Now = new(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);

    private readonly LotFactory lotFactory = new(TimeProvider.System, new AuctionRules());

    public static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    public static LotDraftInput DraftInput(short categoryId = 1) => new(
        categoryId,
        "Vinyl record",
        "Rare vinyl record in a good condition",
        LotCondition.Used,
        500,
        50,
        1440,
        "https://example.com/donate");

    public async Task MigrateAsync()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AuctionDbContext>().Database.MigrateAsync();
    }

    public async Task<User> NewUserAsync()
    {
        var suffix = Suffix();
        var user = User.Register($"user{suffix}@example.com", $"u{suffix}", "hash", Now);
        await SaveAsync(user);
        return user;
    }

    public async Task<Lot> NewLotAsync(long? sellerId = null, LotDraftInput? input = null)
    {
        var seller = sellerId ?? (await NewUserAsync()).Id;
        var lot = lotFactory.CreateDraft(seller, input ?? DraftInput()).Value;
        await SaveAsync(lot);
        return lot;
    }

    public async Task SaveAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        db.Set<TEntity>().Add(entity);
        await db.SaveChangesAsync();
    }

    public async Task<TEntity> LoadAsync<TEntity>(long id)
        where TEntity : Entity
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        return await db.Set<TEntity>().AsNoTracking().SingleAsync(e => e.Id == id);
    }

    public async Task UpdateAsync<TEntity>(long id, Action<TEntity> change)
        where TEntity : Entity
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        var entity = await db.Set<TEntity>().SingleAsync(e => e.Id == id);
        change(entity);
        await db.SaveChangesAsync();
    }

    public async Task ExecuteAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<string>> QueryColumnAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var values = new List<string>();
        while (await reader.ReadAsync())
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }

    public static async Task AssertViolationAsync(Func<Task> action, string sqlState)
    {
        var exception = await Assert.ThrowsAnyAsync<Exception>(action);
        var postgres = exception as PostgresException ?? exception.InnerException as PostgresException;

        Assert.NotNull(postgres);
        Assert.Equal(sqlState, postgres.SqlState);
    }
}
