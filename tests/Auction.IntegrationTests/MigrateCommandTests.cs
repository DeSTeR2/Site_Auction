// The migrate deployment command against a real PostgreSQL server without the application database.
using System.Data;
using Auction.Api.Cli;
using Auction.Infrastructure;
using Auction.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Npgsql;

namespace Auction.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class MigrateCommandTests(PostgresApiFixture fixture)
{
    [Fact]
    public async Task Migrate_creates_missing_database_and_exits_with_zero()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(fixture.ConnectionString)
        {
            Database = $"auction_migrate_{Guid.NewGuid():N}"
        }.ConnectionString;

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Test" });
        builder.Configuration["ConnectionStrings:Postgres"] = connectionString;
        builder.Services.AddInfrastructure(builder.Configuration);
        await using var app = builder.Build();

        var exitCode = await CliCommands.RunAsync(app, CliCommands.Migrate);

        Assert.Equal(0, exitCode);
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task Migrate_is_idempotent()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Test" });
        builder.Configuration["ConnectionStrings:Postgres"] = fixture.ConnectionString;
        builder.Services.AddInfrastructure(builder.Configuration);
        await using var app = builder.Build();

        Assert.Equal(0, await CliCommands.RunAsync(app, CliCommands.Migrate));
        Assert.Equal(0, await CliCommands.RunAsync(app, CliCommands.Migrate));
    }
}
