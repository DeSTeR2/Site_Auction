// Guards the database conventions on the whole EF model: snake_case names and text storage for every enum property.
using System.Text.RegularExpressions;
using Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Auction.UnitTests.Infrastructure;

public class ModelConventionTests
{
    private static readonly Regex SnakeCase = new("^[a-z][a-z0-9_]*$");

    private static readonly IModel Model = BuildModel();

    [Fact]
    public void Model_contains_entities()
    {
        Assert.NotEmpty(Model.GetEntityTypes());
    }

    [Fact]
    public void Every_enum_property_is_stored_as_text()
    {
        var enumProperties = Model.GetEntityTypes()
            .SelectMany(entity => entity.GetProperties())
            .Where(property => (Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType).IsEnum)
            .ToList();

        Assert.NotEmpty(enumProperties);
        Assert.All(enumProperties, property => Assert.Equal(typeof(string), property.GetValueConverter()?.ProviderClrType));
    }

    [Fact]
    public void Table_and_column_names_are_snake_case()
    {
        var names = new List<string>();
        foreach (var entity in Model.GetEntityTypes())
        {
            var table = entity.GetTableName()!;
            names.Add(table);
            var store = StoreObjectIdentifier.Table(table, entity.GetSchema());
            names.AddRange(entity.GetProperties().Select(property => property.GetColumnName(store)!));
        }

        Assert.All(names, name => Assert.Matches(SnakeCase, name));
    }

    [Fact]
    public void Every_entity_has_an_explicit_plural_table_name()
    {
        var tables = Model.GetEntityTypes().Select(entity => entity.GetTableName()!).ToList();

        Assert.All(tables, table => Assert.EndsWith("s", table));
        Assert.Equal(tables.Count, tables.Distinct().Count());
    }

    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseNpgsql("Host=localhost;Database=model_check")
            .UseSnakeCaseNamingConvention()
            .Options;

        using var context = new AuctionDbContext(options);
        return context.GetService<IDesignTimeModel>().Model;
    }
}
