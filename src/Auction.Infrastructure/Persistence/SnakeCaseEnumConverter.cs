// Stores every enum as snake_case text and reads it back (design/03-data-model.md §1).
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Auction.Infrastructure.Persistence;

public static class SnakeCaseEnumConverter
{
    public static ValueConverter For(Type enumType) =>
        (ValueConverter)Activator.CreateInstance(typeof(SnakeCaseEnumConverter<>).MakeGenericType(enumType))!;

    internal static string ToSnakeCase(string name) =>
        Regex.Replace(name, "(?<!^)([A-Z])", "_$1").ToLowerInvariant();
}

public sealed class SnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    private static readonly Dictionary<TEnum, string> ToText = Enum.GetValues<TEnum>()
        .ToDictionary(value => value, value => SnakeCaseEnumConverter.ToSnakeCase(value.ToString()));

    private static readonly Dictionary<string, TEnum> FromText = ToText.ToDictionary(pair => pair.Value, pair => pair.Key);

    public SnakeCaseEnumConverter()
        : base(value => ToText[value], text => FromText[text])
    {
    }
}
